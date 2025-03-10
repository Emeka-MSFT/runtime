// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace System.Text.Json
{
    internal static partial class ThrowHelper
    {
        [DoesNotReturn]
        public static void ThrowArgumentException_DeserializeWrongType(Type type, object value)
        {
            throw new ArgumentException(SR.Format(SR.DeserializeWrongType, type, value.GetType()));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_SerializationNotSupported(Type propertyType)
        {
            throw new NotSupportedException(SR.Format(SR.SerializationNotSupportedType, propertyType));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_TypeRequiresAsyncSerialization(Type propertyType)
        {
            throw new NotSupportedException(SR.Format(SR.TypeRequiresAsyncSerialization, propertyType));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_ConstructorMaxOf64Parameters(Type type)
        {
            throw new NotSupportedException(SR.Format(SR.ConstructorMaxOf64Parameters, type));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_DictionaryKeyTypeNotSupported(Type keyType, JsonConverter converter)
        {
            throw new NotSupportedException(SR.Format(SR.DictionaryKeyTypeNotSupported, keyType, converter.GetType()));
        }

        [DoesNotReturn]
        public static void ThrowJsonException_DeserializeUnableToConvertValue(Type propertyType)
        {
            throw new JsonException(SR.Format(SR.DeserializeUnableToConvertValue, propertyType)) { AppendPathInformation = true };
        }

        [DoesNotReturn]
        public static void ThrowInvalidCastException_DeserializeUnableToAssignValue(Type typeOfValue, Type declaredType)
        {
            throw new InvalidCastException(SR.Format(SR.DeserializeUnableToAssignValue, typeOfValue, declaredType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_DeserializeUnableToAssignNull(Type declaredType)
        {
            throw new InvalidOperationException(SR.Format(SR.DeserializeUnableToAssignNull, declaredType));
        }

        [DoesNotReturn]
        public static void ThrowJsonException_SerializationConverterRead(JsonConverter? converter)
        {
            throw new JsonException(SR.Format(SR.SerializationConverterRead, converter)) { AppendPathInformation = true };
        }

        [DoesNotReturn]
        public static void ThrowJsonException_SerializationConverterWrite(JsonConverter? converter)
        {
            throw new JsonException(SR.Format(SR.SerializationConverterWrite, converter)) { AppendPathInformation = true };
        }

        [DoesNotReturn]
        public static void ThrowJsonException_SerializerCycleDetected(int maxDepth)
        {
            throw new JsonException(SR.Format(SR.SerializerCycleDetected, maxDepth)) { AppendPathInformation = true };
        }

        [DoesNotReturn]
        public static void ThrowJsonException(string? message = null)
        {
            throw new JsonException(message) { AppendPathInformation = true };
        }

        [DoesNotReturn]
        public static void ThrowArgumentException_CannotSerializeInvalidType(string paramName, Type typeToConvert, Type? declaringType, string? propertyName)
        {
            if (declaringType == null)
            {
                Debug.Assert(propertyName == null);
                throw new ArgumentException(SR.Format(SR.CannotSerializeInvalidType, typeToConvert), paramName);
            }

            Debug.Assert(propertyName != null);
            throw new ArgumentException(SR.Format(SR.CannotSerializeInvalidMember, typeToConvert, propertyName, declaringType), paramName);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_CannotSerializeInvalidType(Type typeToConvert, Type? declaringType, MemberInfo? memberInfo)
        {
            if (declaringType == null)
            {
                Debug.Assert(memberInfo == null);
                throw new InvalidOperationException(SR.Format(SR.CannotSerializeInvalidType, typeToConvert));
            }

            Debug.Assert(memberInfo != null);
            throw new InvalidOperationException(SR.Format(SR.CannotSerializeInvalidMember, typeToConvert, memberInfo.Name, declaringType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializationConverterNotCompatible(Type converterType, Type type)
        {
            throw new InvalidOperationException(SR.Format(SR.SerializationConverterNotCompatible, converterType, type));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ResolverTypeNotCompatible(Type requestedType, Type actualType)
        {
            throw new InvalidOperationException(SR.Format(SR.ResolverTypeNotCompatible, actualType, requestedType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ResolverTypeInfoOptionsNotCompatible()
        {
            throw new InvalidOperationException(SR.ResolverTypeInfoOptionsNotCompatible);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_JsonTypeInfoUsedButTypeInfoResolverNotSet()
        {
            throw new InvalidOperationException(SR.JsonTypeInfoUsedButTypeInfoResolverNotSet);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializationConverterOnAttributeInvalid(Type classType, MemberInfo? memberInfo)
        {
            string location = classType.ToString();
            if (memberInfo != null)
            {
                location += $".{memberInfo.Name}";
            }

            throw new InvalidOperationException(SR.Format(SR.SerializationConverterOnAttributeInvalid, location));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializationConverterOnAttributeNotCompatible(Type classTypeAttributeIsOn, MemberInfo? memberInfo, Type typeToConvert)
        {
            string location = classTypeAttributeIsOn.ToString();

            if (memberInfo != null)
            {
                location += $".{memberInfo.Name}";
            }

            throw new InvalidOperationException(SR.Format(SR.SerializationConverterOnAttributeNotCompatible, location, typeToConvert));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializerOptionsImmutable(JsonSerializerContext? context)
        {
            string message = context == null
                ? SR.SerializerOptionsImmutable
                : SR.SerializerContextOptionsImmutable;

            throw new InvalidOperationException(message);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializerContextOptionsImmutable()
        {
            throw new InvalidOperationException(SR.SerializerContextOptionsImmutable);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_TypeInfoResolverImmutable()
        {
            throw new InvalidOperationException(SR.TypeInfoResolverImmutable);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_TypeInfoImmutable()
        {
            throw new InvalidOperationException(SR.TypeInfoImmutable);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_PropertyInfoImmutable()
        {
            throw new InvalidOperationException(SR.PropertyInfoImmutable);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializerPropertyNameConflict(Type type, string propertyName)
        {
            throw new InvalidOperationException(SR.Format(SR.SerializerPropertyNameConflict, type, propertyName));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializerPropertyNameNull(JsonPropertyInfo jsonPropertyInfo)
        {
            throw new InvalidOperationException(SR.Format(SR.SerializerPropertyNameNull, jsonPropertyInfo.DeclaringType, jsonPropertyInfo.MemberName));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NamingPolicyReturnNull(JsonNamingPolicy namingPolicy)
        {
            throw new InvalidOperationException(SR.Format(SR.NamingPolicyReturnNull, namingPolicy));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializerConverterFactoryReturnsNull(Type converterType)
        {
            throw new InvalidOperationException(SR.Format(SR.SerializerConverterFactoryReturnsNull, converterType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializerConverterFactoryReturnsJsonConverterFactorty(Type converterType)
        {
            throw new InvalidOperationException(SR.Format(SR.SerializerConverterFactoryReturnsJsonConverterFactory, converterType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_MultiplePropertiesBindToConstructorParameters(
            Type parentType,
            string parameterName,
            string firstMatchName,
            string secondMatchName)
        {
            throw new InvalidOperationException(
                SR.Format(
                    SR.MultipleMembersBindWithConstructorParameter,
                    firstMatchName,
                    secondMatchName,
                    parentType,
                    parameterName));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ConstructorParameterIncompleteBinding(Type parentType)
        {
            throw new InvalidOperationException(SR.Format(SR.ConstructorParamIncompleteBinding, parentType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ExtensionDataCannotBindToCtorParam(string propertyName, JsonPropertyInfo jsonPropertyInfo)
        {
            throw new InvalidOperationException(SR.Format(SR.ExtensionDataCannotBindToCtorParam, propertyName, jsonPropertyInfo.DeclaringType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_JsonIncludeOnNonPublicInvalid(string memberName, Type declaringType)
        {
            throw new InvalidOperationException(SR.Format(SR.JsonIncludeOnNonPublicInvalid, memberName, declaringType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_IgnoreConditionOnValueTypeInvalid(string clrPropertyName, Type propertyDeclaringType)
        {
            throw new InvalidOperationException(SR.Format(SR.IgnoreConditionOnValueTypeInvalid, clrPropertyName, propertyDeclaringType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NumberHandlingOnPropertyInvalid(JsonPropertyInfo jsonPropertyInfo)
        {
            Debug.Assert(!jsonPropertyInfo.IsForTypeInfo);
            throw new InvalidOperationException(SR.Format(SR.NumberHandlingOnPropertyInvalid, jsonPropertyInfo.MemberName, jsonPropertyInfo.DeclaringType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ConverterCanConvertMultipleTypes(Type runtimePropertyType, JsonConverter jsonConverter)
        {
            throw new InvalidOperationException(SR.Format(SR.ConverterCanConvertMultipleTypes, jsonConverter.GetType(), jsonConverter.TypeToConvert, runtimePropertyType));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_ObjectWithParameterizedCtorRefMetadataNotSupported(
            ReadOnlySpan<byte> propertyName,
            ref Utf8JsonReader reader,
            ref ReadStack state)
        {
            JsonTypeInfo jsonTypeInfo = state.GetTopJsonTypeInfoWithParameterizedConstructor();
            state.Current.JsonPropertyName = propertyName.ToArray();

            NotSupportedException ex = new NotSupportedException(
                SR.Format(SR.ObjectWithParameterizedCtorRefMetadataNotSupported, jsonTypeInfo.Type));
            ThrowNotSupportedException(ref state, reader, ex);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_JsonTypeInfoOperationNotPossibleForKind(JsonTypeInfoKind kind)
        {
            throw new InvalidOperationException(SR.Format(SR.InvalidJsonTypeInfoOperationForKind, kind));
        }

        [DoesNotReturn]
        public static void ReThrowWithPath(ref ReadStack state, JsonReaderException ex)
        {
            Debug.Assert(ex.Path == null);

            string path = state.JsonPath();
            string message = ex.Message;

            // Insert the "Path" portion before "LineNumber" and "BytePositionInLine".
#if BUILDING_INBOX_LIBRARY
            int iPos = message.AsSpan().LastIndexOf(" LineNumber: ");
#else
            int iPos = message.LastIndexOf(" LineNumber: ", StringComparison.InvariantCulture);
#endif
            if (iPos >= 0)
            {
                message = $"{message.Substring(0, iPos)} Path: {path} |{message.Substring(iPos)}";
            }
            else
            {
                message += $" Path: {path}.";
            }

            throw new JsonException(message, path, ex.LineNumber, ex.BytePositionInLine, ex);
        }

        [DoesNotReturn]
        public static void ReThrowWithPath(ref ReadStack state, in Utf8JsonReader reader, Exception ex)
        {
            JsonException jsonException = new JsonException(null, ex);
            AddJsonExceptionInformation(ref state, reader, jsonException);
            throw jsonException;
        }

        public static void AddJsonExceptionInformation(ref ReadStack state, in Utf8JsonReader reader, JsonException ex)
        {
            Debug.Assert(ex.Path is null); // do not overwrite existing path information

            long lineNumber = reader.CurrentState._lineNumber;
            ex.LineNumber = lineNumber;

            long bytePositionInLine = reader.CurrentState._bytePositionInLine;
            ex.BytePositionInLine = bytePositionInLine;

            string path = state.JsonPath();
            ex.Path = path;

            string? message = ex._message;

            if (string.IsNullOrEmpty(message))
            {
                // Use a default message.
                Type propertyType = state.Current.JsonPropertyInfo?.PropertyType ?? state.Current.JsonTypeInfo.Type;
                message = SR.Format(SR.DeserializeUnableToConvertValue, propertyType);
                ex.AppendPathInformation = true;
            }

            if (ex.AppendPathInformation)
            {
                message += $" Path: {path} | LineNumber: {lineNumber} | BytePositionInLine: {bytePositionInLine}.";
                ex.SetMessage(message);
            }
        }

        [DoesNotReturn]
        public static void ReThrowWithPath(ref WriteStack state, Exception ex)
        {
            JsonException jsonException = new JsonException(null, ex);
            AddJsonExceptionInformation(ref state, jsonException);
            throw jsonException;
        }

        public static void AddJsonExceptionInformation(ref WriteStack state, JsonException ex)
        {
            Debug.Assert(ex.Path is null); // do not overwrite existing path information

            string path = state.PropertyPath();
            ex.Path = path;

            string? message = ex._message;
            if (string.IsNullOrEmpty(message))
            {
                // Use a default message.
                message = SR.Format(SR.SerializeUnableToSerialize);
                ex.AppendPathInformation = true;
            }

            if (ex.AppendPathInformation)
            {
                message += $" Path: {path}.";
                ex.SetMessage(message);
            }
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializationDuplicateAttribute(Type attribute, MemberInfo memberInfo)
        {
            string location = memberInfo is Type type ? type.ToString() : $"{memberInfo.DeclaringType}.{memberInfo.Name}";
            throw new InvalidOperationException(SR.Format(SR.SerializationDuplicateAttribute, attribute, location));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializationDuplicateTypeAttribute(Type classType, Type attribute)
        {
            throw new InvalidOperationException(SR.Format(SR.SerializationDuplicateTypeAttribute, classType, attribute));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializationDuplicateTypeAttribute<TAttribute>(Type classType)
        {
            throw new InvalidOperationException(SR.Format(SR.SerializationDuplicateTypeAttribute, classType, typeof(TAttribute)));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializationDataExtensionPropertyInvalid(JsonPropertyInfo jsonPropertyInfo)
        {
            throw new InvalidOperationException(SR.Format(SR.SerializationDataExtensionPropertyInvalid, jsonPropertyInfo.PropertyType, jsonPropertyInfo.MemberName));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException(ref ReadStack state, in Utf8JsonReader reader, NotSupportedException ex)
        {
            string message = ex.Message;

            // The caller should check to ensure path is not already set.
            Debug.Assert(!message.Contains(" Path: "));

            // Obtain the type to show in the message.
            Type propertyType = state.Current.JsonPropertyInfo?.PropertyType ?? state.Current.JsonTypeInfo.Type;

            if (!message.Contains(propertyType.ToString()))
            {
                if (message.Length > 0)
                {
                    message += " ";
                }

                message += SR.Format(SR.SerializationNotSupportedParentType, propertyType);
            }

            long lineNumber = reader.CurrentState._lineNumber;
            long bytePositionInLine = reader.CurrentState._bytePositionInLine;
            message += $" Path: {state.JsonPath()} | LineNumber: {lineNumber} | BytePositionInLine: {bytePositionInLine}.";

            throw new NotSupportedException(message, ex);
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException(ref WriteStack state, NotSupportedException ex)
        {
            string message = ex.Message;

            // The caller should check to ensure path is not already set.
            Debug.Assert(!message.Contains(" Path: "));

            // Obtain the type to show in the message.
            Type propertyType = state.Current.JsonPropertyInfo?.PropertyType ?? state.Current.JsonTypeInfo.Type;

            if (!message.Contains(propertyType.ToString()))
            {
                if (message.Length > 0)
                {
                    message += " ";
                }

                message += SR.Format(SR.SerializationNotSupportedParentType, propertyType);
            }

            message += $" Path: {state.PropertyPath()}.";

            throw new NotSupportedException(message, ex);
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_DeserializeNoConstructor(Type type, ref Utf8JsonReader reader, ref ReadStack state)
        {
            string message;

            if (type.IsInterface)
            {
                message = SR.Format(SR.DeserializePolymorphicInterface, type);
            }
            else
            {
                message = SR.Format(SR.DeserializeNoConstructor, nameof(JsonConstructorAttribute), type);
            }

            ThrowNotSupportedException(ref state, reader, new NotSupportedException(message));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_CannotPopulateCollection(Type type, ref Utf8JsonReader reader, ref ReadStack state)
        {
            ThrowNotSupportedException(ref state, reader, new NotSupportedException(SR.Format(SR.CannotPopulateCollection, type)));
        }

        [DoesNotReturn]
        public static void ThrowJsonException_MetadataValuesInvalidToken(JsonTokenType tokenType)
        {
            ThrowJsonException(SR.Format(SR.MetadataInvalidTokenAfterValues, tokenType));
        }

        [DoesNotReturn]
        public static void ThrowJsonException_MetadataReferenceNotFound(string id)
        {
            ThrowJsonException(SR.Format(SR.MetadataReferenceNotFound, id));
        }

        [DoesNotReturn]
        public static void ThrowJsonException_MetadataValueWasNotString(JsonTokenType tokenType)
        {
            ThrowJsonException(SR.Format(SR.MetadataValueWasNotString, tokenType));
        }

        [DoesNotReturn]
        public static void ThrowJsonException_MetadataValueWasNotString(JsonValueKind valueKind)
        {
            ThrowJsonException(SR.Format(SR.MetadataValueWasNotString, valueKind));
        }

        [DoesNotReturn]
        public static void ThrowJsonException_MetadataReferenceObjectCannotContainOtherProperties(ReadOnlySpan<byte> propertyName, ref ReadStack state)
        {
            state.Current.JsonPropertyName = propertyName.ToArray();
            ThrowJsonException_MetadataReferenceObjectCannotContainOtherProperties();
        }

        [DoesNotReturn]
        public static void ThrowJsonException_MetadataUnexpectedProperty(ReadOnlySpan<byte> propertyName, ref ReadStack state)
        {
            state.Current.JsonPropertyName = propertyName.ToArray();
            ThrowJsonException(SR.Format(SR.MetadataUnexpectedProperty));
        }

        [DoesNotReturn]
        public static void ThrowJsonException_MetadataReferenceObjectCannotContainOtherProperties()
        {
            ThrowJsonException(SR.MetadataReferenceCannotContainOtherProperties);
        }

        [DoesNotReturn]
        public static void ThrowJsonException_MetadataIdIsNotFirstProperty(ReadOnlySpan<byte> propertyName, ref ReadStack state)
        {
            state.Current.JsonPropertyName = propertyName.ToArray();
            ThrowJsonException(SR.MetadataIdIsNotFirstProperty);
        }

        [DoesNotReturn]
        public static void ThrowJsonException_MetadataStandaloneValuesProperty(ref ReadStack state, ReadOnlySpan<byte> propertyName)
        {
            state.Current.JsonPropertyName = propertyName.ToArray();
            ThrowJsonException(SR.MetadataStandaloneValuesProperty);
        }

        [DoesNotReturn]
        public static void ThrowJsonException_MetadataInvalidPropertyWithLeadingDollarSign(ReadOnlySpan<byte> propertyName, ref ReadStack state, in Utf8JsonReader reader)
        {
            // Set PropertyInfo or KeyName to write down the conflicting property name in JsonException.Path
            if (state.Current.IsProcessingDictionary())
            {
                state.Current.JsonPropertyNameAsString = reader.GetString();
            }
            else
            {
                state.Current.JsonPropertyName = propertyName.ToArray();
            }

            ThrowJsonException(SR.MetadataInvalidPropertyWithLeadingDollarSign);
        }

        [DoesNotReturn]
        public static void ThrowJsonException_MetadataDuplicateIdFound(string id)
        {
            ThrowJsonException(SR.Format(SR.MetadataDuplicateIdFound, id));
        }

        [DoesNotReturn]
        public static void ThrowJsonException_MetadataDuplicateTypeProperty()
        {
            ThrowJsonException(SR.MetadataDuplicateTypeProperty);
        }

        [DoesNotReturn]
        public static void ThrowJsonException_MetadataInvalidReferenceToValueType(Type propertyType)
        {
            ThrowJsonException(SR.Format(SR.MetadataInvalidReferenceToValueType, propertyType));
        }

        [DoesNotReturn]
        public static void ThrowJsonException_MetadataInvalidPropertyInArrayMetadata(ref ReadStack state, Type propertyType, in Utf8JsonReader reader)
        {
            state.Current.JsonPropertyName = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan.ToArray();
            string propertyNameAsString = reader.GetString()!;

            ThrowJsonException(SR.Format(SR.MetadataPreservedArrayFailed,
                SR.Format(SR.MetadataInvalidPropertyInArrayMetadata, propertyNameAsString),
                SR.Format(SR.DeserializeUnableToConvertValue, propertyType)));
        }

        [DoesNotReturn]
        public static void ThrowJsonException_MetadataPreservedArrayValuesNotFound(ref ReadStack state, Type propertyType)
        {
            // Missing $values, JSON path should point to the property's object.
            state.Current.JsonPropertyName = null;

            ThrowJsonException(SR.Format(SR.MetadataPreservedArrayFailed,
                SR.MetadataStandaloneValuesProperty,
                SR.Format(SR.DeserializeUnableToConvertValue, propertyType)));
        }

        [DoesNotReturn]
        public static void ThrowJsonException_MetadataCannotParsePreservedObjectIntoImmutable(Type propertyType)
        {
            ThrowJsonException(SR.Format(SR.MetadataCannotParsePreservedObjectToImmutable, propertyType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_MetadataReferenceOfTypeCannotBeAssignedToType(string referenceId, Type currentType, Type typeToConvert)
        {
            throw new InvalidOperationException(SR.Format(SR.MetadataReferenceOfTypeCannotBeAssignedToType, referenceId, currentType, typeToConvert));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_JsonPropertyInfoIsBoundToDifferentJsonTypeInfo(JsonPropertyInfo propertyInfo)
        {
            Debug.Assert(propertyInfo.ParentTypeInfo != null, "We should not throw this exception when ParentTypeInfo is null");
            throw new InvalidOperationException(SR.Format(SR.JsonPropertyInfoBoundToDifferentParent, propertyInfo.Name, propertyInfo.ParentTypeInfo.Type.FullName));
        }

        [DoesNotReturn]
        internal static void ThrowUnexpectedMetadataException(
            ReadOnlySpan<byte> propertyName,
            ref Utf8JsonReader reader,
            ref ReadStack state)
        {

            MetadataPropertyName name = JsonSerializer.GetMetadataPropertyName(propertyName, state.Current.BaseJsonTypeInfo.PolymorphicTypeResolver);
            if (name != 0)
            {
                ThrowJsonException_MetadataUnexpectedProperty(propertyName, ref state);
            }
            else
            {
                ThrowJsonException_MetadataInvalidPropertyWithLeadingDollarSign(propertyName, ref state, reader);
            }
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_BuiltInConvertersNotRooted(Type type)
        {
            throw new NotSupportedException(SR.Format(SR.BuiltInConvertersNotRooted, type));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_NoMetadataForType(Type type)
        {
            throw new NotSupportedException(SR.Format(SR.NoMetadataForType, type));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_NoMetadataForType(Type type)
        {
            throw new InvalidOperationException(SR.Format(SR.NoMetadataForType, type));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_MetadatInitFuncsNull()
        {
            throw new InvalidOperationException(SR.Format(SR.MetadataInitFuncsNull));
        }

        public static void ThrowInvalidOperationException_NoMetadataForTypeProperties(IJsonTypeInfoResolver? resolver, Type type)
        {
            throw new InvalidOperationException(SR.Format(SR.NoMetadataForTypeProperties, resolver?.GetType().FullName ?? "<null>", type));
        }

        public static void ThrowInvalidOperationException_NoMetadataForTypeCtorParams(IJsonTypeInfoResolver? resolver, Type type)
        {
            throw new InvalidOperationException(SR.Format(SR.NoMetadataForTypeCtorParams, resolver?.GetType().FullName ?? "<null>", type));
        }

        public static void ThrowInvalidOperationException_NoDefaultOptionsForContext(JsonSerializerContext context, Type type)
        {
            throw new InvalidOperationException(SR.Format(SR.NoDefaultOptionsForContext, context.GetType(), type));
        }

        [DoesNotReturn]
        public static void ThrowMissingMemberException_MissingFSharpCoreMember(string missingFsharpCoreMember)
        {
            throw new MissingMemberException(SR.Format(SR.MissingFSharpCoreMember, missingFsharpCoreMember));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_BaseConverterDoesNotSupportMetadata(Type derivedType)
        {
            throw new NotSupportedException(SR.Format(SR.Polymorphism_DerivedConverterDoesNotSupportMetadata, derivedType));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_DerivedConverterDoesNotSupportMetadata(Type derivedType)
        {
            throw new NotSupportedException(SR.Format(SR.Polymorphism_DerivedConverterDoesNotSupportMetadata, derivedType));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_RuntimeTypeNotSupported(Type baseType, Type runtimeType)
        {
            throw new NotSupportedException(SR.Format(SR.Polymorphism_RuntimeTypeNotSupported, runtimeType, baseType));
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException_RuntimeTypeDiamondAmbiguity(Type baseType, Type runtimeType, Type derivedType1, Type derivedType2)
        {
            throw new NotSupportedException(SR.Format(SR.Polymorphism_RuntimeTypeDiamondAmbiguity, runtimeType, derivedType1, derivedType2, baseType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_TypeDoesNotSupportPolymorphism(Type baseType)
        {
            throw new InvalidOperationException(SR.Format(SR.Polymorphism_TypeDoesNotSupportPolymorphism, baseType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_DerivedTypeNotSupported(Type baseType, Type derivedType)
        {
            throw new InvalidOperationException(SR.Format(SR.Polymorphism_DerivedTypeIsNotSupported, derivedType, baseType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_DerivedTypeIsAlreadySpecified(Type baseType, Type derivedType)
        {
            throw new InvalidOperationException(SR.Format(SR.Polymorphism_DerivedTypeIsAlreadySpecified, baseType, derivedType));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_TypeDicriminatorIdIsAlreadySpecified(Type baseType, object typeDiscriminator)
        {
            throw new InvalidOperationException(SR.Format(SR.Polymorphism_TypeDicriminatorIdIsAlreadySpecified, baseType, typeDiscriminator));
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_InvalidCustomTypeDiscriminatorPropertyName()
        {
            throw new InvalidOperationException(SR.Polymorphism_InvalidCustomTypeDiscriminatorPropertyName);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_PolymorphicTypeConfigurationDoesNotSpecifyDerivedTypes(Type baseType)
        {
            throw new InvalidOperationException(SR.Format(SR.Polymorphism_ConfigurationDoesNotSpecifyDerivedTypes, baseType));
        }

        [DoesNotReturn]
        public static void ThrowJsonException_UnrecognizedTypeDiscriminator(object typeDiscriminator)
        {
            ThrowJsonException(SR.Format(SR.Polymorphism_UnrecognizedTypeDiscriminator, typeDiscriminator));
        }

        [DoesNotReturn]
        public static void ThrowArgumentException_JsonPolymorphismOptionsAssociatedWithDifferentJsonTypeInfo(string parameterName)
        {
            throw new ArgumentException(SR.JsonPolymorphismOptionsAssociatedWithDifferentJsonTypeInfo, paramName: parameterName);
        }
    }
}
