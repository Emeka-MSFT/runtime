// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import { PromiseControl, promise_control_symbol, create_cancelable_promise } from "./cancelable-promise";
import cwraps from "./cwraps";
import { _lookup_js_owned_object, mono_wasm_get_jsobj_from_js_handle, mono_wasm_get_js_handle, setup_managed_proxy } from "./gc-handles";
import { Module, runtimeHelpers } from "./imports";
import {
    ManagedObject, JSMarshalerArgument, ManagedError, JSMarshalerArguments, MarshalerToCs, MarshalerToJs, JSMarshalerType,
    get_arg_gc_handle, get_arg_js_handle, get_arg_type, get_arg_i32, get_arg_f64, get_arg_i52, get_arg_i16, get_arg_u8, get_arg_f32,
    get_arg_b8, get_arg_date, get_arg_length, set_js_handle, get_arg, set_arg_type,
    get_signature_arg2_type, get_signature_arg1_type, get_signature_type, cs_to_js_marshalers, js_to_cs_marshalers,
    get_signature_res_type, JavaScriptMarshalerArgSize, set_gc_handle, is_args_exception, get_arg_u16, array_element_size, get_string_root, ArraySegment, Span, MemoryViewType, get_signature_arg3_type, MarshalerType, get_arg_i64_big, get_arg_intptr, get_arg_element_type
} from "./marshal";
import { conv_string, conv_string_root } from "./strings";
import { mono_assert, JSHandleNull, GCHandleNull } from "./types";
import { TypedArray } from "./types/emscripten";

export function initialize_marshalers_to_js(): void {
    if (cs_to_js_marshalers.size == 0) {
        cs_to_js_marshalers.set(MarshalerType.Array, _marshal_array_to_js);
        cs_to_js_marshalers.set(MarshalerType.Span, _marshal_span_to_js);
        cs_to_js_marshalers.set(MarshalerType.ArraySegment, _marshal_array_segment_to_js);
        cs_to_js_marshalers.set(MarshalerType.Boolean, _marshal_bool_to_js);
        cs_to_js_marshalers.set(MarshalerType.Byte, _marshal_byte_to_js);
        cs_to_js_marshalers.set(MarshalerType.Char, _marshal_char_to_js);
        cs_to_js_marshalers.set(MarshalerType.Int16, _marshal_int16_to_js);
        cs_to_js_marshalers.set(MarshalerType.Int32, _marshal_int32_to_js);
        cs_to_js_marshalers.set(MarshalerType.Int52, _marshal_int52_to_js);
        cs_to_js_marshalers.set(MarshalerType.BigInt64, _marshal_bigint64_to_js);
        cs_to_js_marshalers.set(MarshalerType.Single, _marshal_float_to_js);
        cs_to_js_marshalers.set(MarshalerType.IntPtr, _marshal_intptr_to_js);
        cs_to_js_marshalers.set(MarshalerType.Double, _marshal_double_to_js);
        cs_to_js_marshalers.set(MarshalerType.String, _marshal_string_to_js);
        cs_to_js_marshalers.set(MarshalerType.Exception, marshal_exception_to_js);
        cs_to_js_marshalers.set(MarshalerType.JSException, marshal_exception_to_js);
        cs_to_js_marshalers.set(MarshalerType.JSObject, _marshal_js_object_to_js);
        cs_to_js_marshalers.set(MarshalerType.Object, _marshal_cs_object_to_js);
        cs_to_js_marshalers.set(MarshalerType.DateTime, _marshal_datetime_to_js);
        cs_to_js_marshalers.set(MarshalerType.DateTimeOffset, _marshal_datetime_to_js);
        cs_to_js_marshalers.set(MarshalerType.Task, _marshal_task_to_js);
        cs_to_js_marshalers.set(MarshalerType.Action, _marshal_delegate_to_js);
        cs_to_js_marshalers.set(MarshalerType.Function, _marshal_delegate_to_js);
        cs_to_js_marshalers.set(MarshalerType.None, _marshal_null_to_js);
        cs_to_js_marshalers.set(MarshalerType.Void, _marshal_null_to_js);
        cs_to_js_marshalers.set(MarshalerType.Discard, _marshal_null_to_js);
    }
}

// eslint-disable-next-line @typescript-eslint/explicit-module-boundary-types
export function generate_arg_marshal_to_js(sig: JSMarshalerType, index: number, arg_offset: number, sig_offset: number, jsname: string, closure: any): {
    converters: string,
    call_body: string,
    marshaler_type: MarshalerType
} {
    let converters = "";
    let converter_types = "";
    let call_body = "";
    const converter_name = "converter" + index;
    let converter_name_arg1 = "null";
    let converter_name_arg2 = "null";
    let converter_name_arg3 = "null";
    let converter_name_res = "null";

    let marshaler_type = get_signature_type(sig);
    if (marshaler_type === MarshalerType.None || marshaler_type === MarshalerType.Void) {
        return {
            converters,
            call_body,
            marshaler_type
        };
    }

    const marshaler_type_res = get_signature_res_type(sig);
    if (marshaler_type_res !== MarshalerType.None) {
        const converter = cs_to_js_marshalers.get(marshaler_type_res);
        mono_assert(converter && typeof converter === "function", () => `Unknow converter for type ${marshaler_type_res} at ${index}`);

        if (marshaler_type != MarshalerType.Nullable) {
            converter_name_res = "converter" + index + "_res";
            converters += ", " + converter_name_res;
            converter_types += " " + MarshalerType[marshaler_type_res];
            closure[converter_name_res] = converter;
        } else {
            marshaler_type = marshaler_type_res;
        }
    }

    const marshaler_type_arg1 = get_signature_arg1_type(sig);
    if (marshaler_type_arg1 !== MarshalerType.None) {
        const converter = js_to_cs_marshalers.get(marshaler_type_arg1);
        mono_assert(converter && typeof converter === "function", () => `Unknow converter for type ${marshaler_type_arg1} at ${index}`);

        converter_name_arg1 = "converter" + index + "_arg1";
        converters += ", " + converter_name_arg1;
        converter_types += " " + MarshalerType[marshaler_type_arg1];
        closure[converter_name_arg1] = converter;
    }

    const marshaler_type_arg2 = get_signature_arg2_type(sig);
    if (marshaler_type_arg2 !== MarshalerType.None) {
        const converter = js_to_cs_marshalers.get(marshaler_type_arg2);
        mono_assert(converter && typeof converter === "function", () => `Unknow converter for type ${marshaler_type_arg2} at ${index}`);

        converter_name_arg2 = "converter" + index + "_arg2";
        converters += ", " + converter_name_arg2;
        converter_types += " " + MarshalerType[marshaler_type_arg2];
        closure[converter_name_arg2] = converter;
    }

    const marshaler_type_arg3 = get_signature_arg3_type(sig);
    if (marshaler_type_arg3 !== MarshalerType.None) {
        const converter = js_to_cs_marshalers.get(marshaler_type_arg3);
        mono_assert(converter && typeof converter === "function", () => `Unknow converter for type ${marshaler_type_arg3} at ${index}`);

        converter_name_arg3 = "converter" + index + "_arg3";
        converters += ", " + converter_name_arg3;
        converter_types += " " + MarshalerType[marshaler_type_arg3];
        closure[converter_name_arg3] = converter;
    }

    const converter = cs_to_js_marshalers.get(marshaler_type);
    mono_assert(converter && typeof converter === "function", () => `Unknow converter for type ${marshaler_type} at ${index} `);

    converters += ", " + converter_name;
    converter_types += " " + MarshalerType[marshaler_type];
    closure[converter_name] = converter;

    if (marshaler_type == MarshalerType.Task) {
        call_body = `  const ${jsname} = ${converter_name}(args + ${arg_offset}, signature + ${sig_offset}, ${converter_name_res}); // ${converter_types} \n`;
    } else if (marshaler_type == MarshalerType.Action || marshaler_type == MarshalerType.Function) {
        call_body = `  const ${jsname} = ${converter_name}(args + ${arg_offset}, signature + ${sig_offset}, ${converter_name_res}, ${converter_name_arg1}, ${converter_name_arg2}, ${converter_name_arg3}); // ${converter_types} \n`;
    } else {
        call_body = `  const ${jsname} = ${converter_name}(args + ${arg_offset}, signature + ${sig_offset}); // ${converter_types} \n`;
    }

    return {
        converters,
        call_body,
        marshaler_type
    };
}

function _marshal_bool_to_js(arg: JSMarshalerArgument): boolean | null {
    const type = get_arg_type(arg);
    if (type == MarshalerType.None) {
        return null;
    }
    return get_arg_b8(arg);
}

function _marshal_byte_to_js(arg: JSMarshalerArgument): number | null {
    const type = get_arg_type(arg);
    if (type == MarshalerType.None) {
        return null;
    }
    return get_arg_u8(arg);
}

function _marshal_char_to_js(arg: JSMarshalerArgument): number | null {
    const type = get_arg_type(arg);
    if (type == MarshalerType.None) {
        return null;
    }
    return get_arg_u16(arg);
}

function _marshal_int16_to_js(arg: JSMarshalerArgument): number | null {
    const type = get_arg_type(arg);
    if (type == MarshalerType.None) {
        return null;
    }
    return get_arg_i16(arg);
}

function _marshal_int32_to_js(arg: JSMarshalerArgument): number | null {
    const type = get_arg_type(arg);
    if (type == MarshalerType.None) {
        return null;
    }
    return get_arg_i32(arg);
}

function _marshal_int52_to_js(arg: JSMarshalerArgument): number | null {
    const type = get_arg_type(arg);
    if (type == MarshalerType.None) {
        return null;
    }
    return get_arg_i52(arg);
}

function _marshal_bigint64_to_js(arg: JSMarshalerArgument): bigint | null {
    const type = get_arg_type(arg);
    if (type == MarshalerType.None) {
        return null;
    }
    return get_arg_i64_big(arg);
}

function _marshal_float_to_js(arg: JSMarshalerArgument): number | null {
    const type = get_arg_type(arg);
    if (type == MarshalerType.None) {
        return null;
    }
    return get_arg_f32(arg);
}

function _marshal_double_to_js(arg: JSMarshalerArgument): number | null {
    const type = get_arg_type(arg);
    if (type == MarshalerType.None) {
        return null;
    }
    return get_arg_f64(arg);
}

function _marshal_intptr_to_js(arg: JSMarshalerArgument): number | null {
    const type = get_arg_type(arg);
    if (type == MarshalerType.None) {
        return null;
    }
    return get_arg_intptr(arg);
}

function _marshal_null_to_js(): null {
    return null;
}

function _marshal_datetime_to_js(arg: JSMarshalerArgument): Date | null {
    const type = get_arg_type(arg);
    if (type === MarshalerType.None) {
        return null;
    }
    return get_arg_date(arg);
}

function _marshal_delegate_to_js(arg: JSMarshalerArgument, _?: JSMarshalerType, res_converter?: MarshalerToJs, arg1_converter?: MarshalerToCs, arg2_converter?: MarshalerToCs, arg3_converter?: MarshalerToCs): Function | null {
    const type = get_arg_type(arg);
    if (type === MarshalerType.None) {
        return null;
    }

    const anyModule = Module as any;
    const gc_handle = get_arg_gc_handle(arg);
    let result = _lookup_js_owned_object(gc_handle);
    if (result === null || result === undefined) {
        // this will create new Function for the C# delegate
        result = (arg1_js: any, arg2_js: any, arg3_js: any) => {

            const sp = anyModule.stackSave();
            try {
                const args = anyModule.stackAlloc(JavaScriptMarshalerArgSize * 5);
                const exc = get_arg(args, 0);
                set_arg_type(exc, MarshalerType.None);
                const res = get_arg(args, 1);
                set_arg_type(res, MarshalerType.None);
                set_gc_handle(res, <any>gc_handle);
                const arg1 = get_arg(args, 2);
                const arg2 = get_arg(args, 3);
                const arg3 = get_arg(args, 4);

                if (arg1_converter) {
                    arg1_converter(arg1, arg1_js);
                }
                if (arg2_converter) {
                    arg2_converter(arg2, arg2_js);
                }
                if (arg3_converter) {
                    arg3_converter(arg3, arg3_js);
                }

                const fail = cwraps.mono_wasm_invoke_method_bound(runtimeHelpers.call_delegate, args);
                if (fail) throw new Error("ERR23: Unexpected error: " + conv_string(fail));
                if (is_args_exception(args)) throw marshal_exception_to_js(exc);

                if (res_converter) {
                    return res_converter(res);
                }

            } finally {
                anyModule.stackRestore(sp);
            }
        };

        setup_managed_proxy(result, gc_handle);
    }

    return result;
}

function _marshal_task_to_js(arg: JSMarshalerArgument, _?: JSMarshalerType, res_converter?: MarshalerToJs): Promise<any> | null {
    const type = get_arg_type(arg);
    if (type === MarshalerType.None) {
        return null;
    }

    if (type !== MarshalerType.Task) {

        if (!res_converter) {
            // when we arrived here from _marshal_cs_object_to_js
            res_converter = cs_to_js_marshalers.get(type);
        }
        mono_assert(res_converter, () => `Unknow sub_converter for type ${MarshalerType[type]} `);

        // this is already resolved
        const val = res_converter(arg);
        return new Promise((resolve) => resolve(val));
    }

    const js_handle = get_arg_js_handle(arg);
    // console.log("_marshal_task_to_js A" + js_handle);
    if (js_handle == JSHandleNull) {
        // this is already resolved void
        return new Promise((resolve) => resolve(undefined));
    }
    const promise = mono_wasm_get_jsobj_from_js_handle(js_handle);
    mono_assert(!!promise, () => `ERR28: promise not found for js_handle: ${js_handle} `);
    const promise_control = promise[promise_control_symbol] as PromiseControl;
    mono_assert(!!promise_control, () => `ERR27: promise_control not found for js_handle: ${js_handle} `);

    const orig_resolve = promise_control.resolve;
    promise_control.resolve = (argInner: JSMarshalerArgument) => {
        // console.log("_marshal_task_to_js R" + js_handle);
        const type = get_arg_type(argInner);
        if (type === MarshalerType.None) {
            orig_resolve(null);
            return;
        }

        if (!res_converter) {
            // when we arrived here from _marshal_cs_object_to_js
            res_converter = cs_to_js_marshalers.get(type);
        }
        mono_assert(res_converter, () => `Unknow sub_converter for type ${MarshalerType[type]}`);

        const js_value = res_converter!(argInner);
        orig_resolve(js_value);
    };

    return promise;
}

export function mono_wasm_marshal_promise(args: JSMarshalerArguments): void {
    const exc = get_arg(args, 0);
    const res = get_arg(args, 1);
    const arg_handle = get_arg(args, 2);
    const arg_value = get_arg(args, 3);

    const exc_type = get_arg_type(exc);
    const value_type = get_arg_type(arg_value);
    const js_handle = get_arg_js_handle(arg_handle);

    if (js_handle === JSHandleNull) {
        const { promise, promise_control } = create_cancelable_promise();
        const js_handle = mono_wasm_get_js_handle(promise)!;
        set_js_handle(res, js_handle);

        if (exc_type !== MarshalerType.None) {
            // this is already failed task
            const reason = marshal_exception_to_js(exc);
            promise_control.reject(reason);
        }
        else if (value_type !== MarshalerType.Task) {
            // this is already resolved task
            const sub_converter = cs_to_js_marshalers.get(value_type);
            mono_assert(sub_converter, () => `Unknow sub_converter for type ${MarshalerType[value_type]} `);
            const data = sub_converter(arg_value);
            promise_control.resolve(data);
        }
    } else {
        // resolve existing promise
        const promise = mono_wasm_get_jsobj_from_js_handle(js_handle);
        mono_assert(!!promise, () => `ERR25: promise not found for js_handle: ${js_handle} `);
        const promise_control = promise[promise_control_symbol] as PromiseControl;
        mono_assert(!!promise_control, () => `ERR26: promise_control not found for js_handle: ${js_handle} `);

        if (exc_type !== MarshalerType.None) {
            const reason = marshal_exception_to_js(exc);
            promise_control.reject(reason);
        }
        else if (value_type !== MarshalerType.Task) {
            // here we assume that resolve was wrapped with sub_converter inside _marshal_task_to_js
            promise_control.resolve(arg_value);
        }
    }
    set_arg_type(res, MarshalerType.Task);
    set_arg_type(exc, MarshalerType.None);
}

function _marshal_string_to_js(arg: JSMarshalerArgument): string | null {
    const type = get_arg_type(arg);
    if (type == MarshalerType.None) {
        return null;
    }
    const root = get_string_root(arg);
    try {
        const value = conv_string_root(root);
        return value;
    } finally {
        root.release();
    }
}

export function marshal_exception_to_js(arg: JSMarshalerArgument): Error | null {
    const type = get_arg_type(arg);
    if (type == MarshalerType.None) {
        return null;
    }
    if (type == MarshalerType.JSException) {
        // this is JSException roundtrip
        const js_handle = get_arg_js_handle(arg);
        const js_obj = mono_wasm_get_jsobj_from_js_handle(js_handle);
        return js_obj;
    }

    const gc_handle = get_arg_gc_handle(arg);
    let result = _lookup_js_owned_object(gc_handle);
    if (result === null || result === undefined) {
        // this will create new ManagedError
        const message = _marshal_string_to_js(arg);
        result = new ManagedError(message!);

        setup_managed_proxy(result, gc_handle);
    }

    return result;
}

function _marshal_js_object_to_js(arg: JSMarshalerArgument): any {
    const type = get_arg_type(arg);
    if (type == MarshalerType.None) {
        return null;
    }
    const js_handle = get_arg_js_handle(arg);
    const js_obj = mono_wasm_get_jsobj_from_js_handle(js_handle);
    return js_obj;
}

function _marshal_cs_object_to_js(arg: JSMarshalerArgument): any {
    const marshaler_type = get_arg_type(arg);
    if (marshaler_type == MarshalerType.None) {
        return null;
    }
    if (marshaler_type == MarshalerType.JSObject) {
        const js_handle = get_arg_js_handle(arg);
        const js_obj = mono_wasm_get_jsobj_from_js_handle(js_handle);
        return js_obj;
    }

    if (marshaler_type == MarshalerType.Array) {
        const element_type = get_arg_element_type(arg);
        return _marshal_array_to_js_impl(arg, element_type);
    }

    if (marshaler_type == MarshalerType.Object) {
        const gc_handle = get_arg_gc_handle(arg);
        if (gc_handle === GCHandleNull) {
            return null;
        }

        // see if we have js owned instance for this gc_handle already
        let result = _lookup_js_owned_object(gc_handle);

        // If the JS object for this gc_handle was already collected (or was never created)
        if (!result) {
            result = new ManagedObject();
            setup_managed_proxy(result, gc_handle);
        }

        return result;
    }

    // other types
    const converter = cs_to_js_marshalers.get(marshaler_type);
    mono_assert(converter, () => `Unknow converter for type ${MarshalerType[marshaler_type]}`);
    return converter(arg);
}

function _marshal_array_to_js(arg: JSMarshalerArgument, sig?: JSMarshalerType): Array<any> | TypedArray | null {
    mono_assert(!!sig, "Expected valid sig paramater");
    const element_type = get_signature_arg1_type(sig);
    return _marshal_array_to_js_impl(arg, element_type);
}

function _marshal_array_to_js_impl(arg: JSMarshalerArgument, element_type: MarshalerType): Array<any> | TypedArray | null {
    const type = get_arg_type(arg);
    if (type == MarshalerType.None) {
        return null;
    }
    const elementSize = array_element_size(element_type);
    mono_assert(elementSize != -1, () => `Element type ${MarshalerType[element_type]} not supported`);
    const buffer_ptr = get_arg_intptr(arg);
    const length = get_arg_length(arg);
    let result: Array<any> | TypedArray | null = null;
    if (element_type == MarshalerType.String) {
        result = new Array(length);
        for (let index = 0; index < length; index++) {
            const element_arg = get_arg(<any>buffer_ptr, index);
            result[index] = _marshal_string_to_js(element_arg);
        }
        cwraps.mono_wasm_deregister_root(<any>buffer_ptr);
    }
    else if (element_type == MarshalerType.Object) {
        result = new Array(length);
        for (let index = 0; index < length; index++) {
            const element_arg = get_arg(<any>buffer_ptr, index);
            result[index] = _marshal_cs_object_to_js(element_arg);
        }
        cwraps.mono_wasm_deregister_root(<any>buffer_ptr);
    }
    else if (element_type == MarshalerType.JSObject) {
        result = new Array(length);
        for (let index = 0; index < length; index++) {
            const element_arg = get_arg(<any>buffer_ptr, index);
            result[index] = _marshal_js_object_to_js(element_arg);
        }
    }
    else if (element_type == MarshalerType.Byte) {
        const sourceView = Module.HEAPU8.subarray(<any>buffer_ptr, buffer_ptr + length);
        result = sourceView.slice();//copy
    }
    else if (element_type == MarshalerType.Int32) {
        const sourceView = Module.HEAP32.subarray(buffer_ptr >> 2, (buffer_ptr >> 2) + length);
        result = sourceView.slice();//copy
    }
    else if (element_type == MarshalerType.Double) {
        const sourceView = Module.HEAPF64.subarray(buffer_ptr >> 3, (buffer_ptr >> 3) + length);
        result = sourceView.slice();//copy
    }
    else {
        throw new Error(`NotImplementedException ${MarshalerType[element_type]} `);
    }
    Module._free(<any>buffer_ptr);
    return result;
}

function _marshal_span_to_js(arg: JSMarshalerArgument, sig?: JSMarshalerType): Span {
    mono_assert(!!sig, "Expected valid sig paramater");

    const element_type = get_signature_arg1_type(sig);
    const buffer_ptr = get_arg_intptr(arg);
    const length = get_arg_length(arg);
    let result: Span | null = null;
    if (element_type == MarshalerType.Byte) {
        result = new Span(<any>buffer_ptr, length, MemoryViewType.Byte);
    }
    else if (element_type == MarshalerType.Int32) {
        result = new Span(<any>buffer_ptr, length, MemoryViewType.Int32);
    }
    else if (element_type == MarshalerType.Double) {
        result = new Span(<any>buffer_ptr, length, MemoryViewType.Double);
    }
    else {
        throw new Error(`NotImplementedException ${MarshalerType[element_type]} `);
    }
    return result;
}

function _marshal_array_segment_to_js(arg: JSMarshalerArgument, sig?: JSMarshalerType): ArraySegment {
    mono_assert(!!sig, "Expected valid sig paramater");

    const element_type = get_signature_arg1_type(sig);
    const buffer_ptr = get_arg_intptr(arg);
    const length = get_arg_length(arg);
    let result: ArraySegment | null = null;
    if (element_type == MarshalerType.Byte) {
        result = new ArraySegment(<any>buffer_ptr, length, MemoryViewType.Byte);
    }
    else if (element_type == MarshalerType.Int32) {
        result = new ArraySegment(<any>buffer_ptr, length, MemoryViewType.Int32);
    }
    else if (element_type == MarshalerType.Double) {
        result = new ArraySegment(<any>buffer_ptr, length, MemoryViewType.Double);
    }
    else {
        throw new Error(`NotImplementedException ${MarshalerType[element_type]} `);
    }
    const gc_handle = get_arg_gc_handle(arg);
    setup_managed_proxy(result, gc_handle);

    return result;
}
