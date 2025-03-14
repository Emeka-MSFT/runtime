// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Quic.Implementations.MsQuic;
using System.Net.Quic.Implementations.MsQuic.Internal;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Quic;
using static Microsoft.Quic.MsQuic;

using NEW_CONNECTION_DATA = Microsoft.Quic.QUIC_LISTENER_EVENT._Anonymous_e__Union._NEW_CONNECTION_e__Struct;
using STOP_COMPLETE_DATA = Microsoft.Quic.QUIC_LISTENER_EVENT._Anonymous_e__Union._STOP_COMPLETE_e__Struct;

namespace System.Net.Quic;

/// <summary>
/// Represents a listener that listens for incoming QUIC connections, see <see href="https://www.rfc-editor.org/rfc/rfc9000.html#name-connections">RFC 9000: Connections</see> for more details.
/// <see cref="QuicListener" /> allows accepting multiple <see cref="QuicConnection" />.
/// </summary>
/// <remarks>
/// Unlike the connection and stream, <see cref="QuicListener" /> lifetime is not linked to any of the accepted connections.
/// It can be safely disposed while keeping the accepted connection alive. The <see cref="DisposeAsync"/> will only stop listening for any other inbound connections.
/// </remarks>
public sealed partial class QuicListener : IAsyncDisposable
{
    /// <summary>
    /// Returns <c>true</c> if QUIC is supported on the current machine and can be used; otherwise, <c>false</c>.
    /// </summary>
    /// <remarks>
    /// The current implementation depends on <see href="https://github.com/microsoft/msquic">MsQuic</see> native library, this property checks its presence (Linux machines).
    /// It also checks whether TLS 1.3, requirement for QUIC protocol, is available and enabled (Windows machines).
    /// </remarks>
    public static bool IsSupported => MsQuicApi.IsQuicSupported;

    /// <summary>
    /// Creates a new <see cref="QuicListener"/> and starts listening for new connections.
    /// </summary>
    /// <param name="options">Options for the listener.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>An asynchronous task that completes with the started listener.</returns>
    public static ValueTask<QuicListener> ListenAsync(QuicListenerOptions options, CancellationToken cancellationToken = default)
    {
        if (!IsSupported)
        {
            throw new PlatformNotSupportedException(SR.SystemNetQuic_PlatformNotSupported);
        }

        // Validate and fill in defaults for the options.
        if (options.ApplicationProtocols.Count <= 0)
        {
            throw new ArgumentException($"Expected at least one item in '{nameof(QuicListenerOptions.ApplicationProtocols)}' to start the listener.", nameof(options));
        }
        if (options.ListenBacklog == 0)
        {
            options.ListenBacklog = 512;
        }

        QuicListener listener = new QuicListener(options);

        if (NetEventSource.Log.IsEnabled())
        {
            NetEventSource.Info(listener, $"Listener listens on {listener.LocalEndPoint}");
        }

        return ValueTask.FromResult(listener);
    }

    /// <summary>
    /// Handle to MsQuic listener object.
    /// </summary>
    private MsQuicContextSafeHandle _handle;

    /// <summary>
    /// Set to non-zero once disposed. Prevents double and/or concurrent disposal.
    /// </summary>
    private int _disposed;

    /// <summary>
    /// Completed when SHUTDOWN_COMPLETE arrives.
    /// </summary>
    private readonly ValueTaskSource _shutdownTcs = new ValueTaskSource();

    /// <summary>
    /// Selects connection options for incoming connections.
    /// </summary>
    private readonly Func<QuicConnection, SslClientHelloInfo, CancellationToken, ValueTask<QuicServerConnectionOptions>> _connectionOptionsCallback;

    /// <summary>
    /// Incoming connections waiting to be accepted via AcceptAsync.
    /// </summary>
    private readonly Channel<PendingConnection> _acceptQueue;

    /// <summary>
    /// The actual listening endpoint.
    /// </summary>
    public IPEndPoint LocalEndPoint { get; }

    public override string ToString() => _handle.ToString();

    private unsafe QuicListener(QuicListenerOptions options)
    {
        GCHandle context = GCHandle.Alloc(this, GCHandleType.Weak);
        try
        {
            QUIC_HANDLE* handle;
            ThrowIfFailure(MsQuicApi.Api.ApiTable->ListenerOpen(
                MsQuicApi.Api.Registration.QuicHandle,
                &NativeCallback,
                (void*)GCHandle.ToIntPtr(context),
                &handle),
                "ListenerOpen failed");
            _handle = new MsQuicContextSafeHandle(handle, context, MsQuicApi.Api.ApiTable->ListenerClose, SafeHandleType.Listener);
        }
        catch
        {
            context.Free();
            throw;
        }

        // Save the connection options before starting the listener
        _connectionOptionsCallback = options.ConnectionOptionsCallback;
        _acceptQueue = Channel.CreateBounded<PendingConnection>(new BoundedChannelOptions(options.ListenBacklog) { SingleWriter = true });

        // Start the listener, from now on MsQuic events will come.
        using MsQuicBuffers alpnBuffers = new MsQuicBuffers();
        alpnBuffers.Initialize(options.ApplicationProtocols, applicationProtocol => applicationProtocol.Protocol);
        QuicAddr address = options.ListenEndPoint.ToQuicAddr();
        if (options.ListenEndPoint.Address.Equals(IPAddress.IPv6Any))
        {
            // For IPv6Any, MsQuic would listen only for IPv6 connections. This would make it impossible
            // to connect the listener by using the IPv4 address (which could have been e.g. resolved by DNS).
            // Using the Unspecified family makes MsQuic handle connections from all IP addresses.
            address.Family = QUIC_ADDRESS_FAMILY_UNSPEC;
        }
        ThrowIfFailure(MsQuicApi.Api.ApiTable->ListenerStart(
            _handle.QuicHandle,
            alpnBuffers.Buffers,
            (uint)alpnBuffers.Count,
            &address),
            "ListenerStart failed");

        // Get the actual listening endpoint.
        LocalEndPoint = MsQuicParameterHelpers.GetIPEndPointParam(MsQuicApi.Api, _handle, QUIC_PARAM_LISTENER_LOCAL_ADDRESS, options.ListenEndPoint.AddressFamily);
    }

    /// <summary>
    /// Accepts an inbound <see cref="QuicConnection" />.
    /// </summary>
    /// <remarks>
    /// Note that <see cref="QuicListener" /> doesn't have a mechanism to report inbound connections that fail the handshake process.
    /// Such connections are only logged by the listener and never surfaced on the outside.
    /// </remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that will contain a fully connected <see cref="QuicConnection" /> which successfully finished the handshake and is ready to be used.</returns>
    public async ValueTask<QuicConnection> AcceptConnectionAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        try
        {
            while (true)
            {
                PendingConnection pendingConnection = await _acceptQueue.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                await using (pendingConnection.ConfigureAwait(false))
                {
                    QuicConnection? connection = await pendingConnection.FinishHandshakeAsync(cancellationToken).ConfigureAwait(false);
                    // Handshake failed, discard this connection and try to get another from the queue.
                    if (connection is null)
                    {
                        continue;
                    }

                    return connection;
                }
            }
        }
        catch (ChannelClosedException ex) when (ex.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }

    private unsafe int HandleEventNewConnection(ref NEW_CONNECTION_DATA data)
    {
        // Check if there's capacity to have another connection waiting to be accepted.
        PendingConnection pendingConnection = new PendingConnection();
        if (!_acceptQueue.Writer.TryWrite(pendingConnection))
        {
            return QUIC_STATUS_CONNECTION_REFUSED;
        }

        QuicConnection connection = new QuicConnection(new MsQuicConnection(data.Connection, data.Info));
        SslClientHelloInfo clientHello = new SslClientHelloInfo(data.Info->ServerNameLength > 0 ? Marshal.PtrToStringUTF8((IntPtr)data.Info->ServerName, data.Info->ServerNameLength) : "", SslProtocols.Tls13);

        // Kicks off the rest of the handshake in the background.
        pendingConnection.StartHandshake(connection, clientHello, _connectionOptionsCallback);

        return QUIC_STATUS_SUCCESS;

    }
    private unsafe int HandleEventStopComplete(ref STOP_COMPLETE_DATA data)
    {
        _shutdownTcs.TrySetResult();
        return QUIC_STATUS_SUCCESS;
    }

    private unsafe int HandleListenerEvent(ref QUIC_LISTENER_EVENT listenerEvent)
        => listenerEvent.Type switch
        {
            QUIC_LISTENER_EVENT_TYPE.NEW_CONNECTION => HandleEventNewConnection(ref listenerEvent.NEW_CONNECTION),
            QUIC_LISTENER_EVENT_TYPE.STOP_COMPLETE => HandleEventStopComplete(ref listenerEvent.STOP_COMPLETE),
            _ => QUIC_STATUS_SUCCESS
        };

#pragma warning disable CS3016
    [UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvCdecl) })]
#pragma warning restore CS3016
    private static unsafe int NativeCallback(QUIC_HANDLE* listener, void* context, QUIC_LISTENER_EVENT* listenerEvent)
    {
        GCHandle stateHandle = GCHandle.FromIntPtr((IntPtr)context);

        // Check if the instance hasn't been collected.
        if (!stateHandle.IsAllocated || stateHandle.Target is not QuicListener instance)
        {
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Error(null, $"Received event {listenerEvent->Type}");
            }
            return QUIC_STATUS_INVALID_STATE;
        }

        try
        {
            // Process the event.
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Info(instance, $"Received event {listenerEvent->Type}");
            }
            return instance.HandleListenerEvent(ref *listenerEvent);
        }
        catch (Exception ex)
        {
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Error(instance, $"Exception while processing event {listenerEvent->Type}: {ex}");
            }
            return QUIC_STATUS_INTERNAL_ERROR;
        }
    }

    /// <summary>
    /// Stops listening for new connections and releases all resources associated with the listener.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        // Check if the listener has been shut down and if not, shut it down.
        if (_shutdownTcs.TryInitialize(out ValueTask valueTask, this))
        {
            unsafe
            {
                MsQuicApi.Api.ApiTable->ListenerStop(_handle.QuicHandle);
            }
        }

        await valueTask.ConfigureAwait(false);
        _handle.Dispose();

        // Flush the queue and dispose all remaining connections.
        _acceptQueue.Writer.TryComplete(ExceptionDispatchInfo.SetCurrentStackTrace(new QuicOperationAbortedException()));
        while (_acceptQueue.Reader.TryRead(out PendingConnection? pendingConnection))
        {
            await pendingConnection.DisposeAsync().ConfigureAwait(false);
        }
    }
}
