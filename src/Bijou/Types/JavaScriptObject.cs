using Bijou.Chakra;
using FluentResults;

namespace Bijou.Types
{
    public class JavaScriptObject
    {
        /// <summary>
        /// The reference.
        /// </summary>
        protected internal JavaScriptValue UnderlyingValue { get; }

        public bool IsValid => UnderlyingValue.IsValid;

        public static JavaScriptObject Invalid { get; } = new JavaScriptObject(JavaScriptValue.Invalid);

        protected internal JavaScriptObject(JavaScriptValue value)
        {
            UnderlyingValue = value;
        }

        public Result<uint> AddRef()
        {
            return NativeMethods.JsAddRef(UnderlyingValue);
        }

        public Result<uint> Release()
        {
            return NativeMethods.JsRelease(UnderlyingValue);
        }

        public Result PreventExtension()
        {
            return NativeMethods.JsPreventExtension(this);
        }

        public Result<bool> Equals(JavaScriptObject other)
        {
            return NativeMethods.JsEquals(UnderlyingValue, other.UnderlyingValue);
        }

        public Result<bool> StrictEquals(JavaScriptObject other)
        {
            return NativeMethods.JsStrictEquals(UnderlyingValue, other.UnderlyingValue);
        }
    }
}