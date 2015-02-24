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

        /// <summary>
        /// Alternative version of <see cref="Type.IsSubclassOf"/> that supports raw generic types (generic types without
        /// any type parameters).
        /// </summary>
        /// <param name="baseType">The base type class for which the check is made.</param>
        /// <param name="toCheck">To type to determine for whether it derives from <paramref name="baseType"/>.</param>
        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type baseType)
        {
            return isSubclassOfRawGeneric(toCheck, baseType);
        }

        static readonly Func<Type, Type, bool> isSubclassOfRawGeneric = FuncExtensions.Memoize((Type toCheck, Type baseType) =>
        {
            if (toCheck != null && toCheck != typeof(object))
            {
                Type cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (baseType == cur || baseType.IsAssignableFrom(cur))
                {
                    return true;
                }

                if (toCheck.BaseType.IsSubclassOfRawGeneric(baseType))
                {
                    return true;
                }

                foreach (var @interface in toCheck.GetInterfaces())
                {
                    if (@interface.IsSubclassOfRawGeneric(baseType))
                    {
                        return true;
                    }
                }
            }

            return false;
        });
    }
}
