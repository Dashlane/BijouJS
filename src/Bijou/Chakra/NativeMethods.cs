﻿using System;
using System.Linq;
using Bijou.Types;
using FluentResults;

namespace Bijou.Chakra
{
    /// <summary>
    /// Native interfaces.
    /// </summary>
    internal static class NativeMethods
    {
        public static Result<JavaScriptRuntime> JsCreateRuntime(
            JavaScriptRuntimeAttributes attributes,
            JavaScriptThreadServiceCallback threadService) =>
            NativeMethodsImpl.JsCreateRuntime(attributes, threadService, out var runtime).ToResult(runtime);

        public static Result JsCollectGarbage(JavaScriptRuntime handle) => 
            NativeMethodsImpl.JsCollectGarbage(handle).ToResult();

        public static Result JsDisposeRuntime(JavaScriptRuntime handle) =>
            NativeMethodsImpl.JsDisposeRuntime(handle).ToResult();

        public static Result<UIntPtr> JsGetRuntimeMemoryUsage(JavaScriptRuntime runtime) =>
            NativeMethodsImpl.JsGetRuntimeMemoryUsage(runtime, out var memoryUsage).ToResult(memoryUsage);

        public static Result<UIntPtr> JsGetRuntimeMemoryLimit(JavaScriptRuntime runtime) =>
            NativeMethodsImpl.JsGetRuntimeMemoryLimit(runtime, out var memoryLimit).ToResult(memoryLimit);

        public static Result JsSetRuntimeMemoryLimit(JavaScriptRuntime runtime, UIntPtr memoryLimit) =>
            NativeMethodsImpl.JsSetRuntimeMemoryLimit(runtime, memoryLimit).ToResult();

        public static Result JsSetRuntimeMemoryAllocationCallback(
            JavaScriptRuntime runtime,
            IntPtr callbackState,
            JavaScriptMemoryAllocationCallback allocationCallback) =>
            NativeMethodsImpl.JsSetRuntimeMemoryAllocationCallback(runtime, callbackState, allocationCallback)
                             .ToResult();


        public static Result JsSetRuntimeBeforeCollectCallback(
            JavaScriptRuntime runtime,
            IntPtr callbackState,
            JavaScriptBeforeCollectCallback beforeCollectCallback) =>
            NativeMethodsImpl.JsSetRuntimeBeforeCollectCallback(runtime, callbackState, beforeCollectCallback)
                             .ToResult();

        public static Result<uint> JsContextAddRef(JavaScriptContext reference) =>
            NativeMethodsImpl.JsContextAddRef(reference, out var count).ToResult(count);

        public static Result<uint> JsAddRef(JavaScriptValue reference) =>
            NativeMethodsImpl.JsAddRef(reference, out var count).ToResult(count);

        public static Result<uint> JsContextRelease(JavaScriptContext reference) =>
            NativeMethodsImpl.JsContextRelease(reference, out var count).ToResult(count);

        public static Result<uint> JsRelease(JavaScriptValue reference) =>
            NativeMethodsImpl.JsRelease(reference, out var count).ToResult(count);

        public static Result<JavaScriptContext> JsCreateContext(JavaScriptRuntime runtime) =>
            NativeMethodsImpl.JsCreateContext(runtime, out var newContext).ToResult(newContext);

        public static Result<JavaScriptContext> JsGetCurrentContext() =>
            NativeMethodsImpl.JsGetCurrentContext(out var currentContext).ToResult(currentContext);

        public static Result JsSetCurrentContext(JavaScriptContext context) =>
            NativeMethodsImpl.JsSetCurrentContext(context).ToResult();

        public static Result<JavaScriptRuntime> JsGetRuntime(JavaScriptContext context) =>
            NativeMethodsImpl.JsGetRuntime(context, out var runtime).ToResult(runtime);

        public static Result JsStartDebugging() => NativeMethodsImpl.JsStartDebugging().ToResult();

        public static Result<uint> JsIdle() =>
            NativeMethodsImpl.JsIdle(out var nextIdleTick).ToResult(nextIdleTick);

        public static Result<JavaScriptFunction> JsParseScript(
            string script,
            JavaScriptSourceContext sourceContext,
            string sourceUrl) => 
            NativeMethodsImpl.JsParseScript(script, sourceContext, sourceUrl, out var value)
                             .ToResult<JavaScriptFunction>(value);

        public static Result<JavaScriptObject> JsRunScript(
            string script,
            JavaScriptSourceContext sourceContext,
            string sourceUrl) => 
            NativeMethodsImpl.JsRunScript(script, sourceContext, sourceUrl, out var value)
                             .ToResult<JavaScriptObject>(value);

        public static Result<ulong> JsSerializeScript(string script, byte[] buffer, ulong bufferSize)
        {
            var size = bufferSize;
            return NativeMethodsImpl.JsSerializeScript(script, buffer, ref size).ToResult(size);
        }

        public static Result<JavaScriptFunction> JsParseSerializedScript(
            string script,
            byte[] buffer,
            JavaScriptSourceContext sourceContext,
            string sourceUrl) => 
            NativeMethodsImpl.JsParseSerializedScript(script, buffer, sourceContext, sourceUrl, out var result)
                             .ToResult<JavaScriptFunction>(result);

        public static Result<JavaScriptObject> JsRunSerializedScript(
            string script,
            byte[] buffer,
            JavaScriptSourceContext sourceContext,
            string sourceUrl) => 
            NativeMethodsImpl.JsRunSerializedScript(script, buffer, sourceContext, sourceUrl, out var result)
                             .ToResult<JavaScriptObject>(result);

        public static Result<JavaScriptPropertyId> JsGetPropertyIdFromName(string name) => 
            NativeMethodsImpl.JsGetPropertyIdFromName(name, out var propertyId).ToResult(propertyId);

        public static Result<string> JsGetPropertyNameFromId(JavaScriptPropertyId propertyId) =>
            NativeMethodsImpl.JsGetPropertyNameFromId(propertyId, out var name).ToResult(name);

        public static Result<JavaScriptObject> JsGetUndefinedValue() =>
            NativeMethodsImpl.JsGetUndefinedValue(out var undefinedValue)
                             .ToResult<JavaScriptObject>(undefinedValue);

        public static Result<JavaScriptObject> JsGetNullValue() =>
            NativeMethodsImpl.JsGetNullValue(out var nullValue).ToResult<JavaScriptObject>(nullValue);

        public static Result<JavaScriptBoolean> JsGetTrueValue() =>
            NativeMethodsImpl.JsGetTrueValue(out var trueValue).ToResult<JavaScriptBoolean>(trueValue);

        public static Result<JavaScriptBoolean> JsGetFalseValue() =>
            NativeMethodsImpl.JsGetFalseValue(out var falseValue).ToResult<JavaScriptBoolean>(falseValue);

        public static Result<JavaScriptBoolean> JsBoolToBoolean(bool value) =>
            NativeMethodsImpl.JsBoolToBoolean(value, out var booleanValue).ToResult<JavaScriptBoolean>(booleanValue);

        public static Result<bool> JsBooleanToBool(JavaScriptBoolean booleanValue) =>
            NativeMethodsImpl.JsBooleanToBool(booleanValue.UnderlyingValue, out var boolValue).ToResult(boolValue);

        public static Result<JavaScriptBoolean> JsConvertValueToBoolean(JavaScriptObject value) =>
            NativeMethodsImpl.JsConvertValueToBoolean(value.UnderlyingValue, out var booleanValue)
                             .ToResult<JavaScriptBoolean>(booleanValue);

        public static Result<JavaScriptValueType> JsGetValueType(JavaScriptValue value) =>
            NativeMethodsImpl.JsGetValueType(value, out var type).ToResult(type);

        public static Result<JavaScriptNumber> JsDoubleToNumber(double doubleValue) =>
            NativeMethodsImpl.JsDoubleToNumber(doubleValue, out var value).ToResult<JavaScriptNumber>(value);

        public static Result<JavaScriptNumber> JsIntToNumber(int intValue) =>
            NativeMethodsImpl.JsIntToNumber(intValue, out var value).ToResult<JavaScriptNumber>(value);

        public static Result<double> JsNumberToDouble(JavaScriptNumber value) =>
            NativeMethodsImpl.JsNumberToDouble(value.UnderlyingValue, out var doubleValue).ToResult(doubleValue);

        public static Result<int> JsNumberToInt(JavaScriptNumber value) =>
            NativeMethodsImpl.JsNumberToInt(value.UnderlyingValue, out var doubleValue).ToResult(doubleValue);

        public static Result<JavaScriptNumber> JsConvertValueToNumber(JavaScriptObject value) =>
            NativeMethodsImpl.JsConvertValueToNumber(value.UnderlyingValue, out var numberValue)
                             .ToResult<JavaScriptNumber>(numberValue);

        public static Result<int> JsGetStringLength(JavaScriptString @string) =>
            NativeMethodsImpl.JsGetStringLength(@string.UnderlyingValue, out var length).ToResult(length);

        public static Result<JavaScriptString> JsPointerToString(string value, UIntPtr stringLength) =>
            NativeMethodsImpl.JsPointerToString(value, stringLength, out var stringValue)
                             .ToResult<JavaScriptString>(stringValue);

        public static Result<(IntPtr Value, UIntPtr Length)> JsStringToPointer(JavaScriptValue value) =>
            NativeMethodsImpl.JsStringToPointer(value, out var stringValue, out var stringLength).ToResult((stringValue, stringLength));

        public static Result<JavaScriptString> JsConvertValueToString(JavaScriptObject value) =>
            NativeMethodsImpl.JsConvertValueToString(value.UnderlyingValue, out var stringValue)
                             .ToResult<JavaScriptString>(stringValue);

        public static Result<JavaScriptObject> JsVariantToValue(ref object var) =>
            NativeMethodsImpl.JsVariantToValue(ref var, out var value).ToResult<JavaScriptObject>(value);

        public static Result<object> JsValueToVariant(JavaScriptValue obj) =>
            NativeMethodsImpl.JsValueToVariant(obj, out var variant).ToResult(variant);

        public static Result<JavaScriptPrototype> JsGetGlobalObject() =>
            NativeMethodsImpl.JsGetGlobalObject(out var globalObject).ToResult<JavaScriptPrototype>(globalObject);

        public static Result<JavaScriptPrototype> JsCreateObject() =>
            NativeMethodsImpl.JsCreateObject(out var obj).ToResult<JavaScriptPrototype>(obj);

        public static Result<JavaScriptObject> JsCreateExternalObject(
            IntPtr data,
            JavaScriptObjectFinalizeCallback finalizeCallback) =>
            NativeMethodsImpl.JsCreateExternalObject(data, finalizeCallback, out var obj)
                             .ToResult<JavaScriptObject>(obj);

        public static Result<JavaScriptObject> JsConvertValueToObject(JavaScriptObject value) =>
            NativeMethodsImpl.JsConvertValueToObject(value.UnderlyingValue, out var obj)
                             .ToResult<JavaScriptObject>(obj);

        public static Result<JavaScriptPrototype> JsGetPrototype(JavaScriptObject obj) =>
            NativeMethodsImpl.JsGetPrototype(obj.UnderlyingValue, out var prototypeObject).ToResult<JavaScriptPrototype>(prototypeObject);

        public static Result JsSetPrototype(JavaScriptPrototype obj, JavaScriptPrototype prototypeObject) =>
            NativeMethodsImpl.JsSetPrototype(obj.UnderlyingValue, prototypeObject.UnderlyingValue).ToResult();

        public static Result<bool> JsGetExtensionAllowed(JavaScriptValue obj) =>
            NativeMethodsImpl.JsGetExtensionAllowed(obj, out var value).ToResult(value);

        public static Result JsPreventExtension(JavaScriptObject obj) =>
            NativeMethodsImpl.JsPreventExtension(obj.UnderlyingValue).ToResult();

        public static Result<TValue> JsGetProperty<TValue>(JavaScriptValue obj, JavaScriptPropertyId propertyId)
            where TValue : JavaScriptObject
        {
            return NativeMethodsImpl.JsGetProperty(obj, propertyId, out var value)
                                    .ToResult<TValue>(value);
        }

        public static Result<JavaScriptObject> JsGetOwnPropertyDescriptor(
            JavaScriptValue obj,
            JavaScriptPropertyId propertyId) =>
            NativeMethodsImpl.JsGetOwnPropertyDescriptor(obj, propertyId, out var propertyDescriptor)
                             .ToResult<JavaScriptObject>(propertyDescriptor);

        public static Result<JavaScriptObject> JsGetOwnPropertyNames(JavaScriptValue obj) =>
            NativeMethodsImpl.JsGetOwnPropertyNames(obj, out var propertyNames)
                             .ToResult<JavaScriptObject>(propertyNames);

        public static Result JsSetProperty(
            JavaScriptValue obj,
            JavaScriptPropertyId propertyId,
            JavaScriptValue value,
            bool useStrictRules) =>
            NativeMethodsImpl.JsSetProperty(obj, propertyId, value, useStrictRules).ToResult();

        public static Result<bool> JsHasProperty(JavaScriptValue obj, JavaScriptPropertyId propertyId) =>
            NativeMethodsImpl.JsHasProperty(obj, propertyId, out var hasProperty).ToResult(hasProperty);

        public static Result<JavaScriptBoolean> JsDeleteProperty(
            JavaScriptValue obj,
            JavaScriptPropertyId propertyId,
            bool useStrictRules) =>
            NativeMethodsImpl.JsDeleteProperty(obj, propertyId, useStrictRules, out var result)
                             .ToResult<JavaScriptBoolean>(result);

        public static Result<bool> JsDefineProperty(
            JavaScriptValue obj,
            JavaScriptPropertyId propertyId,
            JavaScriptValue propertyDescriptor) =>
            NativeMethodsImpl.JsDefineProperty(obj, propertyId, propertyDescriptor, out var result).ToResult(result);

        public static Result<bool> JsHasIndexedProperty(
            JavaScriptObject obj,
            JavaScriptNumber index) =>
            NativeMethodsImpl.JsHasIndexedProperty(obj.UnderlyingValue, index.UnderlyingValue, out var result)
                             .ToResult(result);

        public static Result<TValue> JsGetIndexedProperty<TValue>(
            JavaScriptObject obj,
            JavaScriptNumber index)
            where TValue : JavaScriptObject
        {
            return NativeMethodsImpl.JsGetIndexedProperty(obj.UnderlyingValue, index.UnderlyingValue, out var result)
                                    .ToResult<TValue>(result);
        }

        public static Result JsSetIndexedProperty(
            JavaScriptObject obj,
            JavaScriptNumber index,
            JavaScriptObject value) =>
            NativeMethodsImpl.JsSetIndexedProperty(obj.UnderlyingValue, index.UnderlyingValue, value.UnderlyingValue)
                             .ToResult();

        public static Result JsDeleteIndexedProperty(JavaScriptObject obj, JavaScriptNumber index) =>
            NativeMethodsImpl.JsDeleteIndexedProperty(obj.UnderlyingValue, index.UnderlyingValue)
                             .ToResult();

        public static Result<bool> JsEquals(JavaScriptValue obj1, JavaScriptValue obj2) =>
            NativeMethodsImpl.JsEquals(obj1, obj2, out var result).ToResult(result);

        public static Result<bool> JsStrictEquals(JavaScriptValue obj1, JavaScriptValue obj2) =>
            NativeMethodsImpl.JsStrictEquals(obj1, obj2, out var result).ToResult(result);

        public static Result<bool> JsHasExternalData(JavaScriptValue obj) =>
            NativeMethodsImpl.JsHasExternalData(obj, out var value).ToResult(value);

        public static Result<IntPtr> JsGetExternalData(JavaScriptValue obj) =>
            NativeMethodsImpl.JsGetExternalData(obj, out var externalData).ToResult(externalData);

        public static Result JsSetExternalData(JavaScriptValue obj, IntPtr externalData) =>
            NativeMethodsImpl.JsSetExternalData(obj, externalData).ToResult();

        public static Result<JavaScriptArray> JsCreateArray(uint length) =>
            NativeMethodsImpl.JsCreateArray(length, out var result).ToResult<JavaScriptArray>(result);

        public static Result<TValue> JsCallFunction<TValue>(
            JavaScriptFunction function,
            JavaScriptObject[] arguments,
            ushort argumentCount)
            where TValue : JavaScriptObject
        {
            var parameters = arguments.Select(c => c.UnderlyingValue).ToArray();
            return NativeMethodsImpl.JsCallFunction(function.UnderlyingValue, parameters, argumentCount, out var result)
                                    .ToResult<TValue>(result);
        }

        public static Result<JavaScriptObject> JsConstructObject(
            JavaScriptFunction function,
            JavaScriptObject[] arguments,
            ushort argumentCount)
        {
            var parameters = arguments.Select(c => c.UnderlyingValue).ToArray();
            return NativeMethodsImpl.JsConstructObject(function.UnderlyingValue, parameters, argumentCount, out var result)
                                    .ToResult<JavaScriptObject>(result);
        }

        public static Result<JavaScriptFunction> JsCreateFunction(
            JavaScriptNativeFunction nativeFunction,
            IntPtr externalData) =>
            NativeMethodsImpl.JsCreateFunction(nativeFunction, externalData, out var function)
                             .ToResult<JavaScriptFunction>(function);

        public static Result<JavaScriptError> JsCreateError(JavaScriptString message) =>
            NativeMethodsImpl.JsCreateError(message.UnderlyingValue, out var error)
                             .ToResult<JavaScriptError>(error);

        public static Result<JavaScriptRangeError> JsCreateRangeError(JavaScriptString message) =>
            NativeMethodsImpl.JsCreateRangeError(message.UnderlyingValue, out var error)
                             .ToResult<JavaScriptRangeError>(error);

        public static Result<JavaScriptReferenceError> JsCreateReferenceError(JavaScriptString message) =>
            NativeMethodsImpl.JsCreateReferenceError(message.UnderlyingValue, out var error)
                             .ToResult<JavaScriptReferenceError>(error);

        public static Result<JavaScriptSyntaxError> JsCreateSyntaxError(JavaScriptString message) =>
            NativeMethodsImpl.JsCreateSyntaxError(message.UnderlyingValue, out var error)
                             .ToResult<JavaScriptSyntaxError>(error);

        public static Result<JavaScriptTypeError> JsCreateTypeError(JavaScriptString message) =>
            NativeMethodsImpl.JsCreateTypeError(message.UnderlyingValue, out var error)
                             .ToResult<JavaScriptTypeError>(error);

        public static Result<JavaScriptUriError> JsCreateURIError(JavaScriptString message) =>
            NativeMethodsImpl.JsCreateURIError(message.UnderlyingValue, out var error)
                             .ToResult<JavaScriptUriError>(error);

        public static Result<bool> JsHasException() =>
            NativeMethodsImpl.JsHasException(out var hasException).ToResult(hasException);

        public static Result<JavaScriptObject> JsGetAndClearException() =>
            NativeMethodsImpl.JsGetAndClearException(out var exception)
                             .ToResult<JavaScriptObject>(exception);

        public static Result JsSetException(JavaScriptValue exception) =>
            NativeMethodsImpl.JsSetException(exception).ToResult();

        public static Result JsDisableRuntimeExecution(JavaScriptRuntime runtime) =>
            NativeMethodsImpl.JsDisableRuntimeExecution(runtime).ToResult();

        public static Result JsEnableRuntimeExecution(JavaScriptRuntime runtime) =>
            NativeMethodsImpl.JsEnableRuntimeExecution(runtime).ToResult();

        public static Result<bool> JsIsRuntimeExecutionDisabled(JavaScriptRuntime runtime) =>
            NativeMethodsImpl.JsIsRuntimeExecutionDisabled(runtime, out var isDisabled).ToResult(isDisabled);

        public static Result JsProjectWinRTNamespace(string namespaceName) =>
            NativeMethodsImpl.JsProjectWinRTNamespace(namespaceName).ToResult();

        public static Result JsSetPromiseContinuationCallback(
            JavaScriptPromiseContinuationCallback promiseContinuationCallback,
            IntPtr callbackState) =>
            NativeMethodsImpl.JsSetPromiseContinuationCallback(promiseContinuationCallback, callbackState).ToResult();
    }
}
