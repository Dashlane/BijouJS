using Bijou.Chakra;
using FluentResults;

namespace Bijou.Types
{
    public class JavaScriptArray : JavaScriptObject
    {
        internal JavaScriptArray(JavaScriptValue value) : base(value)
        {
        }

        public static Result<JavaScriptArray> Create(uint length)
        {
            return NativeMethods.JsCreateArray(length);
        }

        public Result<bool> HasIndexedProperty(JavaScriptNumber index)
        {
            return NativeMethods.JsHasIndexedProperty(this, index);
        }

        public Result<TValue> GetIndexedProperty<TValue>(JavaScriptNumber index)
            where TValue : JavaScriptObject
        {
            return NativeMethods.JsGetIndexedProperty<TValue>(this, index);
        }

        public Result SetIndexedProperty(JavaScriptNumber index, JavaScriptObject value)
        {
            return NativeMethods.JsSetIndexedProperty(this, index, value);
        }

        public Result DeleteIndexedProperty(JavaScriptNumber index)
        {
            return NativeMethods.JsDeleteIndexedProperty(this, index);
        }
    }
}
