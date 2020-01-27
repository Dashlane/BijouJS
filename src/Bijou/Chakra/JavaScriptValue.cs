using System;
using FluentResults;

namespace Bijou.Chakra
{
    /// <summary>
    /// A JavaScript value.
    /// </summary>
    /// <remarks>
    /// A JavaScript value is one of the following types of values: Undefined, Null, Boolean, 
    /// String, Number, or Object.
    /// </remarks>
    public struct JavaScriptValue
    {
        private readonly IntPtr _reference;

        /// <summary>
        ///     Gets an invalid value.
        /// </summary>
        public static JavaScriptValue Invalid { get; } = new JavaScriptValue(IntPtr.Zero);

        /// <summary>
        ///     Gets a value indicating whether the value is valid.
        /// </summary>
        public bool IsValid => _reference != IntPtr.Zero;

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

            set => NativeMethods.JsSetExternalData(this, value.Value);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptValue"/> struct.
        /// </summary>
        /// <param name="reference">The reference.</param>
        private JavaScriptValue(IntPtr reference)
        {
            _reference = reference;
        }
    }
}
