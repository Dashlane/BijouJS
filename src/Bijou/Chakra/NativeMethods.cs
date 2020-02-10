using System;
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

        public static Result<JavaScriptValue> JsParseScript(
            string script,
            JavaScriptSourceContext sourceContext,
            string sourceUrl) => 
            NativeMethodsImpl.JsParseScript(script, sourceContext, sourceUrl, out var value)
                             .ToResult(value);

        public static Result<JavaScriptValue> JsRunScript(
            string script,
            JavaScriptSourceContext sourceContext,
            string sourceUrl) => 
            NativeMethodsImpl.JsRunScript(script, sourceContext, sourceUrl, out var value)
                             .ToResult(value);

        public static Result<ulong> JsSerializeScript(string script, byte[] buffer, ulong bufferSize)
        {
            var size = bufferSize;
            return NativeMethodsImpl.JsSerializeScript(script, buffer, ref size).ToResult(size);
        }

        public static Result<JavaScriptValue> JsParseSerializedScript(
            string script,
            byte[] buffer,
            JavaScriptSourceContext sourceContext,
            string sourceUrl) => 
            NativeMethodsImpl.JsParseSerializedScript(script, buffer, sourceContext, sourceUrl, out var result)
                             .ToResult(result);

        public static Result<JavaScriptValue> JsRunSerializedScript(
            string script,
            byte[] buffer,
            JavaScriptSourceContext sourceContext,
            string sourceUrl) => 
            NativeMethodsImpl.JsRunSerializedScript(script, buffer, sourceContext, sourceUrl, out var result)
                             .ToResult(result);

        public static Result<JavaScriptPropertyId> JsGetPropertyIdFromName(string name) => 
            NativeMethodsImpl.JsGetPropertyIdFromName(name, out var propertyId).ToResult(propertyId);

        public static Result<string> JsGetPropertyNameFromId(JavaScriptPropertyId propertyId) =>
            NativeMethodsImpl.JsGetPropertyNameFromId(propertyId, out var name).ToResult(name);

        public static Result<JavaScriptValue> JsGetUndefinedValue() =>
            NativeMethodsImpl.JsGetUndefinedValue(out var undefinedValue)
                             .ToResult(undefinedValue);

        public static Result<JavaScriptValue> JsGetNullValue() =>
            NativeMethodsImpl.JsGetNullValue(out var nullValue)
                             .ToResult(nullValue);

        public static Result<JavaScriptValue> JsGetTrueValue() =>
            NativeMethodsImpl.JsGetTrueValue(out var trueValue)
                             .ToResult(trueValue);

        public static Result<JavaScriptValue> JsGetFalseValue() =>
            NativeMethodsImpl.JsGetFalseValue(out var falseValue)
                             .ToResult(falseValue);

        public static Result<JavaScriptValue> JsBoolToBoolean(bool value) =>
            NativeMethodsImpl.JsBoolToBoolean(value, out var booleanValue)
                             .ToResult(booleanValue);

        public static Result<bool> JsBooleanToBool(JavaScriptValue booleanValue) =>
            NativeMethodsImpl.JsBooleanToBool(booleanValue, out var boolValue)
                             .ToResult(boolValue);

        public static Result<JavaScriptValue> JsConvertValueToBoolean(JavaScriptValue value) =>
            NativeMethodsImpl.JsConvertValueToBoolean(value, out var booleanValue)
                             .ToResult(booleanValue);

        public static Result<JavaScriptValueType> JsGetValueType(JavaScriptValue value) =>
            NativeMethodsImpl.JsGetValueType(value, out var type)
                             .ToResult(type);

        public static Result<JavaScriptValue> JsDoubleToNumber(double doubleValue) =>
            NativeMethodsImpl.JsDoubleToNumber(doubleValue, out var value)
                             .ToResult(value);

        public static Result<JavaScriptValue> JsIntToNumber(int intValue) =>
            NativeMethodsImpl.JsIntToNumber(intValue, out var value)
                             .ToResult(value);

        public static Result<double> JsNumberToDouble(JavaScriptValue value) =>
            NativeMethodsImpl.JsNumberToDouble(value, out var doubleValue)
                             .ToResult(doubleValue);

        public static Result<int> JsNumberToInt(JavaScriptValue value) =>
            NativeMethodsImpl.JsNumberToInt(value, out var doubleValue)
                             .ToResult(doubleValue);

        public static Result<JavaScriptValue> JsConvertValueToNumber(JavaScriptValue value) =>
            NativeMethodsImpl.JsConvertValueToNumber(value, out var numberValue)
                             .ToResult(numberValue);

        public static Result<int> JsGetStringLength(JavaScriptValue @string) =>
            NativeMethodsImpl.JsGetStringLength(@string, out var length)
                             .ToResult(length);

        public static Result<JavaScriptValue> JsPointerToString(string value, UIntPtr stringLength) =>
            NativeMethodsImpl.JsPointerToString(value, stringLength, out var stringValue)
                             .ToResult(stringValue);

        public static Result<(IntPtr Value, UIntPtr Length)> JsStringToPointer(JavaScriptValue value) =>
            NativeMethodsImpl.JsStringToPointer(value, out var stringValue, out var stringLength).ToResult((stringValue, stringLength));

        public static Result<JavaScriptValue> JsConvertValueToString(JavaScriptValue value) =>
            NativeMethodsImpl.JsConvertValueToString(value, out var stringValue)
                             .ToResult(stringValue);

        public static Result<JavaScriptValue> JsVariantToValue(ref object var) =>
            NativeMethodsImpl.JsVariantToValue(ref var, out var value)
                             .ToResult(value);

        public static Result<object> JsValueToVariant(JavaScriptValue obj) =>
            NativeMethodsImpl.JsValueToVariant(obj, out var variant).ToResult(variant);

        public static Result<JavaScriptValue> JsGetGlobalObject()
        {
            var errorCode = NativeMethodsImpl.JsGetGlobalObject(out var globalObject);
            return errorCode.ToResult(globalObject);
        }
            
                             

        public static Result<JavaScriptValue> JsCreateObject() =>
            NativeMethodsImpl.JsCreateObject(out var obj)
                             .ToResult(obj);

        public static Result<JavaScriptValue> JsCreateExternalObject(
            IntPtr data,
            JavaScriptObjectFinalizeCallback finalizeCallback) =>
            NativeMethodsImpl.JsCreateExternalObject(data, finalizeCallback, out var obj)
                             .ToResult(obj);

        public static Result<JavaScriptValue> JsConvertValueToObject(JavaScriptValue value) =>
            NativeMethodsImpl.JsConvertValueToObject(value, out var obj)
                             .ToResult(obj);

        public static Result<JavaScriptValue> JsGetPrototype(JavaScriptValue obj) =>
            NativeMethodsImpl.JsGetPrototype(obj, out var prototypeObject)
                             .ToResult(prototypeObject);

        public static Result JsSetPrototype(JavaScriptValue obj, JavaScriptValue prototypeObject) =>
            NativeMethodsImpl.JsSetPrototype(obj, prototypeObject)
                             .ToResult();

        public static Result<bool> JsGetExtensionAllowed(JavaScriptValue obj) =>
            NativeMethodsImpl.JsGetExtensionAllowed(obj, out var value).ToResult(value);

        public static Result JsPreventExtension(JavaScriptValue obj) =>
            NativeMethodsImpl.JsPreventExtension(obj).ToResult();

        public static Result<JavaScriptValue> JsGetProperty(JavaScriptValue obj, JavaScriptPropertyId propertyId) => 
            NativeMethodsImpl.JsGetProperty(obj, propertyId, out var value).ToResult(value);
        

        public static Result<JavaScriptValue> JsGetOwnPropertyDescriptor(
            JavaScriptValue obj,
            JavaScriptPropertyId propertyId) =>
            NativeMethodsImpl.JsGetOwnPropertyDescriptor(obj, propertyId, out var propertyDescriptor)
                             .ToResult(propertyDescriptor);

        public static Result<JavaScriptValue> JsGetOwnPropertyNames(JavaScriptValue obj) =>
            NativeMethodsImpl.JsGetOwnPropertyNames(obj, out var propertyNames)
                             .ToResult(propertyNames);

        public static Result JsSetProperty(
            JavaScriptValue obj,
            JavaScriptPropertyId propertyId,
            JavaScriptValue value,
            bool useStrictRules) =>
            NativeMethodsImpl.JsSetProperty(obj, propertyId, value, useStrictRules).ToResult();

        public static Result<bool> JsHasProperty(JavaScriptValue obj, JavaScriptPropertyId propertyId) =>
            NativeMethodsImpl.JsHasProperty(obj, propertyId, out var hasProperty).ToResult(hasProperty);

        public static Result<JavaScriptValue> JsDeleteProperty(
            JavaScriptValue obj,
            JavaScriptPropertyId propertyId,
            bool useStrictRules) =>
            NativeMethodsImpl.JsDeleteProperty(obj, propertyId, useStrictRules, out var result)
                             .ToResult(result);

        public static Result<bool> JsDefineProperty(
            JavaScriptValue obj,
            JavaScriptPropertyId propertyId,
            JavaScriptValue propertyDescriptor) =>
            NativeMethodsImpl.JsDefineProperty(obj, propertyId, propertyDescriptor, out var result).ToResult(result);

        public static Result<bool> JsHasIndexedProperty(
            JavaScriptValue obj,
            JavaScriptValue index) =>
            NativeMethodsImpl.JsHasIndexedProperty(obj, index, out var result)
                             .ToResult(result);

        public static Result JsGetIndexedProperty(
            JavaScriptValue obj,
            JavaScriptValue index) =>
            NativeMethodsImpl.JsGetIndexedProperty(obj, index, out var result)
                             .ToResult(result);

        public static Result JsSetIndexedProperty(
            JavaScriptValue obj,
            JavaScriptValue index,
            JavaScriptValue value) =>
            NativeMethodsImpl.JsSetIndexedProperty(obj, index, value)
                             .ToResult();

        public static Result JsDeleteIndexedProperty(JavaScriptValue obj, JavaScriptValue index) =>
            NativeMethodsImpl.JsDeleteIndexedProperty(obj, index)
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

        public static Result<JavaScriptValue> JsCreateArray(uint length) =>
            NativeMethodsImpl.JsCreateArray(length, out var result)
                             .ToResult(result);

        public static Result<JavaScriptValue> JsCallFunction(
            JavaScriptValue function,
            JavaScriptValue[] arguments,
            ushort argumentCount) =>
            NativeMethodsImpl.JsCallFunction(function, arguments, argumentCount, out var result)
                             .ToResult(result);

        public static Result<JavaScriptValue> JsConstructObject(
            JavaScriptValue function,
            JavaScriptValue[] arguments,
            ushort argumentCount) =>
            NativeMethodsImpl.JsConstructObject(function, arguments, argumentCount, out var result)
                             .ToResult(result);

        public static Result<JavaScriptValue> JsCreateFunction(
            JavaScriptNativeFunction nativeFunction,
            IntPtr externalData) =>
            NativeMethodsImpl.JsCreateFunction(nativeFunction, externalData, out var function)
                             .ToResult(function);

        public static Result<JavaScriptValue> JsCreateError(JavaScriptValue message) =>
            NativeMethodsImpl.JsCreateError(message, out var error)
                             .ToResult(error);

        public static Result<JavaScriptValue> JsCreateRangeError(JavaScriptValue message) =>
            NativeMethodsImpl.JsCreateRangeError(message, out var error)
                             .ToResult(error);

        public static Result<JavaScriptValue> JsCreateReferenceError(JavaScriptValue message) =>
            NativeMethodsImpl.JsCreateReferenceError(message, out var error)
                             .ToResult(error);

        public static Result<JavaScriptValue> JsCreateSyntaxError(JavaScriptValue message) =>
            NativeMethodsImpl.JsCreateSyntaxError(message, out var error)
                             .ToResult(error);

        public static Result<JavaScriptValue> JsCreateTypeError(JavaScriptValue message) =>
            NativeMethodsImpl.JsCreateTypeError(message, out var error)
                             .ToResult(error);

        public static Result<JavaScriptValue> JsCreateURIError(JavaScriptValue message) =>
            NativeMethodsImpl.JsCreateURIError(message, out var error)
                             .ToResult(error);

        public static Result<bool> JsHasException() =>
            NativeMethodsImpl.JsHasException(out var hasException).ToResult(hasException);

        public static Result<JavaScriptValue> JsGetAndClearException() =>
            NativeMethodsImpl.JsGetAndClearException(out var exception)
                             .ToResult(exception);

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
