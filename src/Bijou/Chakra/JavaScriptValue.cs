using System;
using System.Linq;
using System.Runtime.InteropServices;
using FluentResults;

namespace Bijou.Chakra
{
    /// <summary>
    ///     A JavaScript value.
    /// </summary>
    /// <remarks>
    ///     A JavaScript value is one of the following types of values: Undefined, Null, Boolean, 
    ///     String, Number, or Object.
    /// </remarks>
    internal struct JavaScriptValue
    {
        /// <summary>
        /// The reference.
        /// </summary>
        private readonly IntPtr reference;

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptValue"/> struct.
        /// </summary>
        /// <param name="reference">The reference.</param>
        private JavaScriptValue(IntPtr reference)
        {
            this.reference = reference;
        }

        /// <summary>
        ///     Gets an invalid value.
        /// </summary>
        public static JavaScriptValue Invalid
        {
            get { return new JavaScriptValue(IntPtr.Zero); }
        }

        /// <summary>
        ///     Gets the value of <c>undefined</c> in the current script context.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        public static Result<JavaScriptValue> Undefined => NativeMethods.JsGetUndefinedValue();

        /// <summary>
        ///     Gets the value of <c>null</c> in the current script context.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        public static Result<JavaScriptValue> Null => NativeMethods.JsGetNullValue();

        /// <summary>
        ///     Gets the value of <c>true</c> in the current script context.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        public static Result<JavaScriptValue> True => NativeMethods.JsGetTrueValue();

        /// <summary>
        ///     Gets the value of <c>false</c> in the current script context.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        public static Result<JavaScriptValue> False => NativeMethods.JsGetFalseValue();

        /// <summary>
        ///     Gets the global object in the current script context.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        public static Result<JavaScriptValue> GlobalObject => NativeMethods.JsGetGlobalObject();

        /// <summary>
        ///     Gets a value indicating whether the value is valid.
        /// </summary>
        public bool IsValid => reference != IntPtr.Zero;

        /// <summary>
        ///     Gets the JavaScript type of the value.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <returns>The type of the value.</returns>
        public Result<JavaScriptValueType> ValueType => NativeMethods.JsGetValueType(this);

        /// <summary>
        ///     Gets the length of a <c>String</c> value.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <returns>The length of the string.</returns>
        public Result<int> StringLength => NativeMethods.JsGetStringLength(this);

        /// <summary>
        ///     Gets or sets the prototype of an object.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        public Result<JavaScriptValue> Prototype
        {
            get => NativeMethods.JsGetPrototype(this);

            set => NativeMethods.JsSetPrototype(this, value.Value);
        }

        /// <summary>
        ///     Gets a value indicating whether an object is extensible or not.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        public Result<bool> IsExtensionAllowed => NativeMethods.JsGetExtensionAllowed(this);

        /// <summary>
        ///     Gets a value indicating whether an object is an external object.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        public Result<bool> HasExternalData => NativeMethods.JsHasExternalData(this);

        /// <summary>
        ///     Gets or sets the data in an external object.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        public Result<IntPtr> ExternalData
        {
            get => NativeMethods.JsGetExternalData(this);

            set
            {
                NativeMethods.JsSetExternalData(this, value.Value);
            }
        }

        /// <summary>
        ///     Creates a <c>Boolean</c> value from a <c>bool</c> value.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="value">The value to be converted.</param>
        /// <returns>The converted value.</returns>
        public static Result<JavaScriptValue> FromBoolean(bool value)
        {
            return NativeMethods.JsBoolToBoolean(value);
        }

        /// <summary>
        ///     Creates a <c>Number</c> value from a <c>double</c> value.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="value">The value to be converted.</param>
        /// <returns>The new <c>Number</c> value.</returns>
        public static Result<JavaScriptValue> FromDouble(double value)
        {
            return NativeMethods.JsDoubleToNumber(value);
        }

        /// <summary>
        ///     Creates a <c>Number</c> value from a <c>int</c> value.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="value">The value to be converted.</param>
        /// <returns>The new <c>Number</c> value.</returns>
        public static Result<JavaScriptValue> FromInt32(int value)
        {
            return NativeMethods.JsIntToNumber(value);
        }

        /// <summary>
        ///     Creates a <c>String</c> value from a string pointer.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="value">The string  to convert to a <c>String</c> value.</param>
        /// <returns>The new <c>String</c> value.</returns>
        public static Result<JavaScriptValue> FromString(string value)
        {
            return NativeMethods.JsPointerToString(value, new UIntPtr((uint)value.Length));
        }

        /// <summary>
        ///     Creates a new <c>Object</c>.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <returns>The new <c>Object</c>.</returns>
        public static Result<JavaScriptValue> CreateObject()
        {
            return NativeMethods.JsCreateObject();
        }

        /// <summary>
        ///     Creates a new <c>Object</c> that stores some external data.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="data">External data that the object will represent. May be null.</param>
        /// <param name="finalizer">
        ///     A callback for when the object is finalized. May be null.
        /// </param>
        /// <returns>The new <c>Object</c>.</returns>
        internal static Result<JavaScriptValue> CreateExternalObject(IntPtr data, JavaScriptObjectFinalizeCallback finalizer)
        {
            return NativeMethods.JsCreateExternalObject(data, finalizer);
        }

        /// <summary>
        ///     Creates a new JavaScript function.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="function">The method to call when the function is invoked.</param>
        /// <returns>The new function object.</returns>
        internal static Result<JavaScriptValue> CreateFunction(JavaScriptNativeFunction function)
        {
            return NativeMethods.JsCreateFunction(function, IntPtr.Zero);
        }

        /// <summary>
        ///     Creates a new JavaScript function.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="function">The method to call when the function is invoked.</param>
        /// <param name="callbackData">Data to be provided to all function callbacks.</param>
        /// <returns>The new function object.</returns>
        internal static Result<JavaScriptValue> CreateFunction(JavaScriptNativeFunction function, IntPtr callbackData)
        {
            return NativeMethods.JsCreateFunction(function, callbackData);
        }

        /// <summary>
        ///     Creates a JavaScript array object.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="length">The initial length of the array.</param>
        /// <returns>The new array object.</returns>
        public static Result<JavaScriptValue> CreateArray(uint length)
        {
            return NativeMethods.JsCreateArray(length);
        }

        /// <summary>
        ///     Creates a new JavaScript error object
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="message">Message for the error object.</param>
        /// <returns>The new error object.</returns>
        public static Result<JavaScriptValue> CreateError(JavaScriptValue message)
        {
            return NativeMethods.JsCreateError(message);
        }

        /// <summary>
        ///     Creates a new JavaScript RangeError error object
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="message">Message for the error object.</param>
        /// <returns>The new error object.</returns>
        public static Result<JavaScriptValue> CreateRangeError(JavaScriptValue message)
        {
            return NativeMethods.JsCreateRangeError(message);
        }

        /// <summary>
        ///     Creates a new JavaScript ReferenceError error object
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="message">Message for the error object.</param>
        /// <returns>The new error object.</returns>
        public static Result<JavaScriptValue> CreateReferenceError(JavaScriptValue message)
        {
            return NativeMethods.JsCreateReferenceError(message);
        }

        /// <summary>
        ///     Creates a new JavaScript SyntaxError error object
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="message">Message for the error object.</param>
        /// <returns>The new error object.</returns>
        public static Result<JavaScriptValue> CreateSyntaxError(JavaScriptValue message)
        {
            return NativeMethods.JsCreateSyntaxError(message);
        }

        /// <summary>
        ///     Creates a new JavaScript TypeError error object
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="message">Message for the error object.</param>
        /// <returns>The new error object.</returns>
        public static Result<JavaScriptValue> CreateTypeError(JavaScriptValue message)
        {
            return NativeMethods.JsCreateTypeError(message);
        }

        /// <summary>
        ///     Creates a new JavaScript URIError error object
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="message">Message for the error object.</param>
        /// <returns>The new error object.</returns>
        public static Result<JavaScriptValue> CreateUriError(JavaScriptValue message)
        {
            return NativeMethods.JsCreateURIError(message);
        }

        /// <summary>
        ///     Adds a reference to the object.
        /// </summary>
        /// <remarks>
        ///     This only needs to be called on objects that are not going to be stored somewhere on 
        ///     the stack. Calling AddRef ensures that the JavaScript object the value refers to will not be freed 
        ///     until Release is called
        /// </remarks>
        /// <returns>The object's new reference count.</returns>
        public Result<uint> AddRef()
        {
            return NativeMethods.JsAddRef(this);
        }

        /// <summary>
        ///     Releases a reference to the object.
        /// </summary>
        /// <remarks>
        ///     Removes a reference that was created by AddRef.
        /// </remarks>
        /// <returns>The object's new reference count.</returns>
        public Result<uint> Release()
        {
            return NativeMethods.JsRelease(this);
        }

        /// <summary>
        ///     Retrieves the <c>bool</c> value of a <c>Boolean</c> value.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <returns>The converted value.</returns>
        public Result<bool> ToBoolean()
        {
            return NativeMethods.JsBooleanToBool(this);
        }

        /// <summary>
        ///     Retrieves the <c>double</c> value of a <c>Number</c> value.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///     This function retrieves the value of a Number value. It will fail with 
        ///     <c>InvalidArgument</c> if the type of the value is not <c>Number</c>.
        ///     </para>
        ///     <para>
        ///     Requires an active script context.
        ///     </para>
        /// </remarks>
        /// <returns>The <c>double</c> value.</returns>
        public Result<double> ToDouble()
        {
            return NativeMethods.JsNumberToDouble(this);
        }

        /// <summary>
        ///     Retrieves the <c>int</c> value of a <c>Number</c> value.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///     This function retrieves the value of a Number value. It will fail with
        ///     <c>InvalidArgument</c> if the type of the value is not <c>Number</c>.
        ///     </para>
        ///     <para>
        ///     Requires an active script context.
        ///     </para>
        /// </remarks>
        /// <returns>The <c>int</c> value.</returns>
        public Result<int> ToInt32()
        {
            return NativeMethods.JsNumberToInt(this);
        }

        /// <summary>
        ///     Retrieves the string pointer of a <c>String</c> value.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///     This function retrieves the string pointer of a <c>String</c> value. It will fail with 
        ///     <c>InvalidArgument</c> if the type of the value is not <c>String</c>.
        ///     </para>
        ///     <para>
        ///     Requires an active script context.
        ///     </para>
        /// </remarks>
        /// <returns>The string.</returns>
        public string AsString()
        {
            var result = NativeMethods.JsStringToPointer(this);
            if (result.IsFailed)
            {
                return result.Errors.First().Message;
            }

            var (buffer, length) = result.Value;
            return Marshal.PtrToStringUni(buffer, (int)length);
        }

        /// <summary>
        ///     Converts the value to <c>Boolean</c> using regular JavaScript semantics.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <returns>The converted value.</returns>
        public Result<JavaScriptValue> ConvertToBoolean()
        {
            return NativeMethods.JsConvertValueToBoolean(this);
        }

        /// <summary>
        ///     Converts the value to <c>Number</c> using regular JavaScript semantics.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <returns>The converted value.</returns>
        public Result<JavaScriptValue> ConvertToNumber()
        {
            return NativeMethods.JsConvertValueToNumber(this);
        }

        /// <summary>
        ///     Converts the value to <c>String</c> using regular JavaScript semantics.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <returns>The converted value.</returns>
        public Result<JavaScriptValue> ConvertToString()
        {
            return NativeMethods.JsConvertValueToString(this);
        }

        /// <summary>
        ///     Converts the value to <c>Object</c> using regular JavaScript semantics.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <returns>The converted value.</returns>
        public Result<JavaScriptValue> ConvertToObject()
        {
            return NativeMethods.JsConvertValueToObject(this);
        }

        /// <summary>
        ///     Sets an object to not be extensible.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        public void PreventExtension()
        {
            NativeMethods.JsPreventExtension(this);
        }

        /// <summary>
        ///     Gets a property descriptor for an object's own property.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="propertyId">The ID of the property.</param>
        /// <returns>The property descriptor.</returns>
        public Result<JavaScriptValue> GetOwnPropertyDescriptor(string propertyId)
        {
            return NativeMethods.JsGetOwnPropertyDescriptor(this, JavaScriptPropertyId.FromString(propertyId).Value);
        }

        /// <summary>
        ///     Gets the list of all properties on the object.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <returns>An array of property names.</returns>
        public Result<JavaScriptValue> GetOwnPropertyNames()
        {
            return NativeMethods.JsGetOwnPropertyNames(this);
        }

        /// <summary>
        ///     Determines whether an object has a property.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="propertyId">The ID of the property.</param>
        /// <returns>Whether the object (or a prototype) has the property.</returns>
        public Result<bool> HasProperty(string propertyId)
        {
            return NativeMethods.JsHasProperty(this, JavaScriptPropertyId.FromString(propertyId).Value);
        }

        /// <summary>
        ///     Gets an object's property.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="id">The ID of the property.</param>
        /// <returns>The value of the property.</returns>
        public Result<JavaScriptValue> GetProperty(string id)
        {
            return NativeMethods.JsGetProperty(this, JavaScriptPropertyId.FromString(id).Value);
        }

        /// <summary>
        ///     Sets an object's property.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="id">The ID of the property.</param>
        /// <param name="value">The new value of the property.</param>
        /// <param name="useStrictRules">The property set should follow strict mode rules.</param>
        public void SetProperty(string id, JavaScriptValue value, bool useStrictRules)
        {
            NativeMethods.JsSetProperty(this, JavaScriptPropertyId.FromString(id).Value, value, useStrictRules);
        }

        /// <summary>
        ///     Deletes an object's property.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="propertyId">The ID of the property.</param>
        /// <param name="useStrictRules">The property set should follow strict mode rules.</param>
        /// <returns>Whether the property was deleted.</returns>
        public Result<JavaScriptValue> DeleteProperty(string propertyId, bool useStrictRules)
        {
            return NativeMethods.JsDeleteProperty(this, JavaScriptPropertyId.FromString(propertyId).Value, useStrictRules);
        }

        /// <summary>
        ///     Defines a new object's own property from a property descriptor.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="propertyId">The ID of the property.</param>
        /// <param name="propertyDescriptor">The property descriptor.</param>
        /// <returns>Whether the property was defined.</returns>
        public Result<bool> DefineProperty(string propertyId, JavaScriptValue propertyDescriptor)
        {
            return NativeMethods.JsDefineProperty(this, JavaScriptPropertyId.FromString(propertyId).Value, propertyDescriptor);
        }

        /// <summary>
        ///     Test if an object has a value at the specified index.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="index">The index to test.</param>
        /// <returns>Whether the object has an value at the specified index.</returns>
        public Result<bool> HasIndexedProperty(JavaScriptValue index)
        {
            return NativeMethods.JsHasIndexedProperty(this, index);
        }

        /// <summary>
        ///     Retrieve the value at the specified index of an object.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="index">The index to retrieve.</param>
        /// <returns>The retrieved value.</returns>
        public Result<JavaScriptValue> GetIndexedProperty(JavaScriptValue index)
        {
            return NativeMethods.JsGetIndexedProperty(this, index);
        }

        /// <summary>
        ///     Set the value at the specified index of an object.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="index">The index to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetIndexedProperty(JavaScriptValue index, JavaScriptValue value)
        {
            NativeMethods.JsSetIndexedProperty(this, index, value);
        }

        /// <summary>
        ///     Delete the value at the specified index of an object.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="index">The index to delete.</param>
        public void DeleteIndexedProperty(JavaScriptValue index)
        {
            NativeMethods.JsDeleteIndexedProperty(this, index);
        }

        /// <summary>
        ///     Compare two JavaScript values for equality.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///     This function is equivalent to the "==" operator in JavaScript.
        ///     </para>
        ///     <para>
        ///     Requires an active script context.
        ///     </para>
        /// </remarks>
        /// <param name="other">The object to compare.</param>
        /// <returns>Whether the values are equal.</returns>
        public bool Equals(JavaScriptValue other)
        {
            return NativeMethods.JsEquals(this, other).ValueOrDefault;
        }

        /// <summary>
        ///     Compare two JavaScript values for strict equality.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///     This function is equivalent to the "===" operator in JavaScript.
        ///     </para>
        ///     <para>
        ///     Requires an active script context.
        ///     </para>
        /// </remarks>
        /// <param name="other">The object to compare.</param>
        /// <returns>Whether the values are strictly equal.</returns>
        public bool StrictEquals(JavaScriptValue other)
        {
            return NativeMethods.JsStrictEquals(this, other).ValueOrDefault;
        }

        /// <summary>
        ///     Invokes a function.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="arguments">The arguments to the call.</param>
        /// <returns>The <c>Value</c> returned from the function invocation, if any.</returns>
        public Result<JavaScriptValue> CallFunction(params JavaScriptValue[] arguments)
        {
            if (arguments.Length > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(arguments));
            }

            var ret = NativeMethods.JsCallFunction(this, arguments, (ushort)arguments.Length);
            return ret;
        }

        /// <summary>
        ///     Invokes a function as a constructor.
        /// </summary>
        /// <remarks>
        ///     Requires an active script context.
        /// </remarks>
        /// <param name="arguments">The arguments to the call.</param>
        /// <returns>The <c>Value</c> returned from the function invocation.</returns>
        public Result<JavaScriptValue> ConstructObject(params JavaScriptValue[] arguments)
        {
            if (arguments.Length > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(arguments));
            }

            return NativeMethods.JsConstructObject(this, arguments, (ushort)arguments.Length);
        }
    }
}
