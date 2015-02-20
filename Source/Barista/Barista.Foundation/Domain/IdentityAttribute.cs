using System;

namespace Barista.Foundation.Domain
{
    /// <summary>
    /// Marks a property as the aggregate root identity.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IdentityAttribute : Attribute
    {
    }
}
