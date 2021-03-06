﻿using System;
using System.Runtime.InteropServices;

namespace Bijou.Chakra
{
    /// <summary>
    /// Native interfaces.
    /// </summary>
    internal static class NativeMethodsImpl
    {
        private const string ChakraDll = "Chakra.dll";

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsCreateRuntime(JavaScriptRuntimeAttributes attributes, JavaScriptThreadServiceCallback threadService, out JavaScriptRuntime runtime);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsCollectGarbage(JavaScriptRuntime handle);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsDisposeRuntime(JavaScriptRuntime handle);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetRuntimeMemoryUsage(JavaScriptRuntime runtime, out UIntPtr memoryUsage);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetRuntimeMemoryLimit(JavaScriptRuntime runtime, out UIntPtr memoryLimit);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsSetRuntimeMemoryLimit(JavaScriptRuntime runtime, UIntPtr memoryLimit);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsSetRuntimeMemoryAllocationCallback(JavaScriptRuntime runtime, IntPtr callbackState, JavaScriptMemoryAllocationCallback allocationCallback);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsSetRuntimeBeforeCollectCallback(JavaScriptRuntime runtime, IntPtr callbackState, JavaScriptBeforeCollectCallback beforeCollectCallback);

        [DllImport(ChakraDll, EntryPoint = "JsAddRef", ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsContextAddRef(JavaScriptContext reference, out uint count);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsAddRef(JavaScriptValue reference, out uint count);

        [DllImport(ChakraDll, EntryPoint = "JsRelease", ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsContextRelease(JavaScriptContext reference, out uint count);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsRelease(JavaScriptValue reference, out uint count);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsCreateContext(JavaScriptRuntime runtime, out JavaScriptContext newContext);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetCurrentContext(out JavaScriptContext currentContext);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsSetCurrentContext(JavaScriptContext context);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetRuntime(JavaScriptContext context, out JavaScriptRuntime runtime);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsStartDebugging();

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsIdle(out uint nextIdleTick);

        [DllImport(ChakraDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsParseScript(string script, JavaScriptSourceContext sourceContext, string sourceUrl, out JavaScriptValue result);

        [DllImport(ChakraDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsRunScript(string script, JavaScriptSourceContext sourceContext, string sourceUrl, out JavaScriptValue result);

        [DllImport(ChakraDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsSerializeScript(string script, byte[] buffer, ref ulong bufferSize);

        [DllImport(ChakraDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsParseSerializedScript(string script, byte[] buffer, JavaScriptSourceContext sourceContext, string sourceUrl, out JavaScriptValue result);

        [DllImport(ChakraDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsRunSerializedScript(string script, byte[] buffer, JavaScriptSourceContext sourceContext, string sourceUrl, out JavaScriptValue result);

        [DllImport(ChakraDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetPropertyIdFromName(string name, out JavaScriptPropertyId propertyId);

        [DllImport(ChakraDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetPropertyNameFromId(JavaScriptPropertyId propertyId, out string name);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetUndefinedValue(out JavaScriptValue undefinedValue);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetNullValue(out JavaScriptValue nullValue);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetTrueValue(out JavaScriptValue trueValue);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetFalseValue(out JavaScriptValue falseValue);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsBoolToBoolean(bool value, out JavaScriptValue booleanValue);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsBooleanToBool(JavaScriptValue booleanValue, out bool boolValue);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsConvertValueToBoolean(JavaScriptValue value, out JavaScriptValue booleanValue);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetValueType(JavaScriptValue value, out JavaScriptValueType type);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsDoubleToNumber(double doubleValue, out JavaScriptValue value);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsIntToNumber(int intValue, out JavaScriptValue value);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsNumberToDouble(JavaScriptValue value, out double doubleValue);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsNumberToInt(JavaScriptValue value, out int doubleValue);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsConvertValueToNumber(JavaScriptValue value, out JavaScriptValue numberValue);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetStringLength(JavaScriptValue stringValue, out int length);

        [DllImport(ChakraDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsPointerToString(string value, UIntPtr stringLength, out JavaScriptValue stringValue);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsStringToPointer(JavaScriptValue value, out IntPtr stringValue, out UIntPtr stringLength);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsConvertValueToString(JavaScriptValue value, out JavaScriptValue stringValue);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsVariantToValue(ref object var, out JavaScriptValue value);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsValueToVariant(JavaScriptValue obj, out object var);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetGlobalObject(out JavaScriptValue globalObject);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsCreateObject(out JavaScriptValue obj);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsCreateExternalObject(IntPtr data, JavaScriptObjectFinalizeCallback finalizeCallback, out JavaScriptValue obj);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsConvertValueToObject(JavaScriptValue value, out JavaScriptValue obj);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetPrototype(JavaScriptValue obj, out JavaScriptValue prototypeObject);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsSetPrototype(JavaScriptValue obj, JavaScriptValue prototypeObject);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetExtensionAllowed(JavaScriptValue obj, out bool value);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsPreventExtension(JavaScriptValue obj);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetProperty(JavaScriptValue obj, JavaScriptPropertyId propertyId, out JavaScriptValue value);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetOwnPropertyDescriptor(JavaScriptValue obj, JavaScriptPropertyId propertyId, out JavaScriptValue propertyDescriptor);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetOwnPropertyNames(JavaScriptValue obj, out JavaScriptValue propertyNames);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsSetProperty(JavaScriptValue obj, JavaScriptPropertyId propertyId, JavaScriptValue value, bool useStrictRules);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsHasProperty(JavaScriptValue obj, JavaScriptPropertyId propertyId, out bool hasProperty);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsDeleteProperty(JavaScriptValue obj, JavaScriptPropertyId propertyId, bool useStrictRules, out JavaScriptValue result);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsDefineProperty(JavaScriptValue obj, JavaScriptPropertyId propertyId, JavaScriptValue propertyDescriptor, out bool result);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsHasIndexedProperty(JavaScriptValue obj, JavaScriptValue index, out bool result);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetIndexedProperty(JavaScriptValue obj, JavaScriptValue index, out JavaScriptValue result);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsSetIndexedProperty(JavaScriptValue obj, JavaScriptValue index, JavaScriptValue value);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsDeleteIndexedProperty(JavaScriptValue obj, JavaScriptValue index);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsEquals(JavaScriptValue obj1, JavaScriptValue obj2, out bool result);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsStrictEquals(JavaScriptValue obj1, JavaScriptValue obj2, out bool result);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsHasExternalData(JavaScriptValue obj, out bool value);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetExternalData(JavaScriptValue obj, out IntPtr externalData);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsSetExternalData(JavaScriptValue obj, IntPtr externalData);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsCreateArray(uint length, out JavaScriptValue result);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsCallFunction(JavaScriptValue function, JavaScriptValue[] arguments, ushort argumentCount, out JavaScriptValue result);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsConstructObject(JavaScriptValue function, JavaScriptValue[] arguments, ushort argumentCount, out JavaScriptValue result);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsCreateFunction(JavaScriptNativeFunction nativeFunction, IntPtr externalData, out JavaScriptValue function);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsCreateError(JavaScriptValue message, out JavaScriptValue error);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsCreateRangeError(JavaScriptValue message, out JavaScriptValue error);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsCreateReferenceError(JavaScriptValue message, out JavaScriptValue error);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsCreateSyntaxError(JavaScriptValue message, out JavaScriptValue error);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsCreateTypeError(JavaScriptValue message, out JavaScriptValue error);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsCreateURIError(JavaScriptValue message, out JavaScriptValue error);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsHasException(out bool hasException);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsGetAndClearException(out JavaScriptValue exception);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsSetException(JavaScriptValue exception);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsDisableRuntimeExecution(JavaScriptRuntime runtime);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsEnableRuntimeExecution(JavaScriptRuntime runtime);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsIsRuntimeExecutionDisabled(JavaScriptRuntime runtime, out bool isDisabled);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsInspectableToObject([MarshalAs(UnmanagedType.IInspectable)] Object inspectable, out JavaScriptValue value);

        [DllImport(ChakraDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsProjectWinRTNamespace(string namespaceName);

        [DllImport(ChakraDll, ExactSpelling = true)]
        internal static extern JavaScriptErrorCode JsSetPromiseContinuationCallback(JavaScriptPromiseContinuationCallback promiseContinuationCallback, IntPtr callbackState);
    }
}
