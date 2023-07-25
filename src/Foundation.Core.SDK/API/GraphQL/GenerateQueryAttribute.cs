using System;

namespace Foundation.Core.SDK.API.GraphQL
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class GenerateQueryAttribute : Attribute
    {
        public GenerateQueryAttribute() { }
    }
}