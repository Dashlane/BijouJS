using System.Linq;
using Bijou.Chakra;
using FluentResults;

namespace Bijou.Types
{
    public class JavaScriptPrototype : JavaScriptObject
    {
        public static Result<JavaScriptPrototype> GlobalObject => NativeMethods.JsGetGlobalObject();

        public Result<JavaScriptPrototype> Prototype
        {
            get => NativeMethods.JsGetPrototype(this);

            set => NativeMethods.JsSetPrototype(this, value.Value);
        }

        internal JavaScriptPrototype(JavaScriptValue value) : base(value)
        {
        }

        public Result<JavaScriptPrototype> Create()
        {
            return NativeMethods.JsCreateObject();
        }

        public Result<bool> DefineProperty(string property, JavaScriptValue propertyDescriptor)
        {
            var propertyId = JavaScriptPropertyId.FromString(property);
            if (propertyId.IsFailed)
            {
                return Results.Fail(propertyId.Errors.First());
            }

            return NativeMethods.JsDefineProperty(UnderlyingValue, propertyId.Value, propertyDescriptor);
        }

        public Result<JavaScriptBoolean> DeleteProperty(string property, bool useStrictRules)
        {
            var propertyId = JavaScriptPropertyId.FromString(property);
            if (propertyId.IsFailed)
            {
                return Results.Fail(propertyId.Errors.First());
            }

            return NativeMethods.JsDeleteProperty(UnderlyingValue, propertyId.Value, useStrictRules);
        }

        public Result<TValue> GetProperty<TValue>(string property) where TValue : JavaScriptObject
        {
            var propertyId = JavaScriptPropertyId.FromString(property);
            if (propertyId.IsFailed)
            {
                return Results.Fail(propertyId.Errors.First());
            }

            var result = NativeMethods.JsGetProperty<TValue>(UnderlyingValue, propertyId.Value);
            if (result.IsFailed)
            {
                return Results.Fail(result.Errors.First());
            }

            return result;
        }

        public Result SetProperty(string property, JavaScriptObject value, bool useStrictRules)
        {
            var propertyId = JavaScriptPropertyId.FromString(property);
            if (propertyId.IsFailed)
            {
                return Results.Fail(propertyId.Errors.First());
            }

            return NativeMethods.JsSetProperty(UnderlyingValue, propertyId.Value, value.UnderlyingValue, useStrictRules);
        }

        public Result<bool> HasProperty(string property)
        {
            var propertyId = JavaScriptPropertyId.FromString(property);
            if (propertyId.IsFailed)
            {
                return Results.Fail(propertyId.Errors.First());
            }

            return NativeMethods.JsHasProperty(UnderlyingValue, propertyId.Value);
        }

        public Result<JavaScriptObject> GetOwnPropertyDescriptor(string property)
        {
            var propertyId = JavaScriptPropertyId.FromString(property);
            if (propertyId.IsFailed)
            {
                return Results.Fail(propertyId.Errors.First());
            }

            return NativeMethods.JsGetOwnPropertyDescriptor(UnderlyingValue, propertyId.Value);
        }

        public Result<JavaScriptObject> GetOwnPropertyNames()
        {
            return NativeMethods.JsGetOwnPropertyNames(UnderlyingValue);
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