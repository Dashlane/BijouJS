using Bijou.Chakra;

namespace Bijou.Types
{
    public sealed class JavaScriptNull : JavaScriptObject
    {
        internal JavaScriptNull(JavaScriptValue value) : base(value)
        {
        }
    }
}
