using System;
using System.Reflection;

namespace Barista.Foundation.Common.Extensions
{
    public static class TypeExtensions
    {
        private static readonly Func<MemberInfo, Type, bool> hasAttribute = FuncExtensions.Memoize(
            (MemberInfo m, Type t) =>
                (m.GetCustomAttributes(t, true).Length > 0));

        public static bool HasAttribute<TAttribute>(this MemberInfo memberInfo) where TAttribute : Attribute
        {
            return hasAttribute(memberInfo, typeof(TAttribute));
        }
    }
}
