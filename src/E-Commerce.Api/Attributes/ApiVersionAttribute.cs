using System;

namespace E_Commerce.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiVersionAttribute : Attribute
    {
    }
}
