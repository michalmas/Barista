using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Barista.Foundation.Domain
{
    /// <summary>
    /// Optimizes the lookup of domain event handlers for aggregates that rely on Event Sourcing
    /// </summary>
    internal static class EventMappingCache
    {
        #region Private Definitions

        private static readonly ConcurrentDictionary<Type, IDictionary<Type, MethodInfo>> cache =
            new ConcurrentDictionary<Type, IDictionary<Type, MethodInfo>>();

        #endregion

        /// <summary>
        /// Finds the appropriate When method that can handle a particular type of event.
        /// </summary>
        public static MethodInfo GetHandlerFor(Type ownerType, Type eventType)
        {
            IDictionary<Type, MethodInfo> methods = cache.GetOrAdd(ownerType, t => EnumerateEventHandlersOf(ownerType));

            if (!methods.ContainsKey(eventType))
            {
                throw new InvalidOperationException(
                    string.Format("Type {0} does not know how to process event {1}", ownerType.FullName, eventType.FullName));
            }

            return methods[eventType];
        }

        private static IDictionary<Type, MethodInfo> EnumerateEventHandlersOf(Type type)
        {
            return type
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.Name == "When")
                .Where(m => m.GetParameters().Length == 1)
                .ToDictionary(m => m.GetParameters().First().ParameterType, m => m);
        }
    }
}
