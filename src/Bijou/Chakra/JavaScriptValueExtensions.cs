using Bijou.Types;

namespace Bijou.Chakra
{
    public static class JavaScriptValueExtensions
    {
        public static JavaScriptObject ToObject(this JavaScriptValue value)
        {
            var type = NativeMethods.JsGetValueType(value);
            if (type.IsFailed)
            {
                return new JavaScriptUndefined(value);
            }

            switch (type.Value)
            {
                case JavaScriptValueType.Undefined:
                    return new JavaScriptUndefined(value);

                case JavaScriptValueType.Null:
                    return new JavaScriptNull(value);

                case JavaScriptValueType.Number:
                    return new JavaScriptNumber(value);

                case JavaScriptValueType.String:
                    return new JavaScriptString(value);

                case JavaScriptValueType.Boolean:
                    return new JavaScriptBoolean(value);

                case JavaScriptValueType.Object:
                    return new JavaScriptPrototype(value);

                case JavaScriptValueType.Function:
                    return new JavaScriptFunction(value);

                case JavaScriptValueType.Error:
                    return new JavaScriptError(value);

                case JavaScriptValueType.Array:
                    return new JavaScriptArray(value);

                default:
                    return new JavaScriptUndefined(value);
            }
        }
    }
}
