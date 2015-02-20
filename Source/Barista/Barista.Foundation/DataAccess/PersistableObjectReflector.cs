using System;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;

using Barista.Foundation.Domain;
using Barista.Foundation.EventSourcing;

using FluentNHibernate.Conventions;

namespace Barista.Foundation.DataAccess
{
    public static class PersistableObjectReflector
    {
        /// <summary>
        /// Gets the single property on the specified <paramref name="aggregateRootType"/> that is decorated
        /// with the <see cref="KeyAttribute"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Will be thrown when the specified <paramref name="aggregateRootType"/> does not implement the
        /// <see cref="IAggregateRoot"/> interface, or when it does not have a single property decorated with
        /// the <see cref=" KeyAttribute"/>.
        /// </exception>
        public static string GetKeyPropertyName(Type aggregateRootType)
        {
            var properties = aggregateRootType
                .GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance)
                .Where(pi => pi.GetCustomAttributes(typeof(IdentityAttribute), true).Any()).ToArray();

            if (properties.Count() != 1)
            {
                throw new InvalidOperationException(string.Format(
                    "Aggregate root {0} must have exactly one property that is marked with [Identity] attribute", aggregateRootType.Name));
            }

            return properties.Single().Name;
        }

        /// <summary>
        /// Gets the functional key of an <see cref="IPersistable"/> from the property that is decorated with the
        /// <see cref="KeyAttribute"/>.
        /// </summary>
        public static object GetKey(IPersistable aggregateRoot)
        {
            Type type = aggregateRoot.GetType();
            string keyPropertyName = GetKeyPropertyName(type);
            return type.GetProperty(keyPropertyName).GetValue(aggregateRoot, null);
        }

        /// <summary>
        /// Sets the functional key of an <see cref="IPersistable"/> from the property that is decorated with the
        /// <see cref="KeyAttribute"/>.
        /// </summary>
        public static void SetKey(IPersistable aggregateRoot, object identity)
        {
            Type type = aggregateRoot.GetType();
            string keyPropertyName = GetKeyPropertyName(type);
            type.GetProperty(keyPropertyName).SetValue(aggregateRoot, identity, null);
        }


        /// <summary>
        /// Gets the prefix to use for calculating a unique but deterministic Guid from an <see cref="EventSource"/>.
        /// </summary>
        public static string GetTypeStreamPrefix(Type aggregateRootType)
        {
            var streamIdAttributes = aggregateRootType.GetCustomAttributes(typeof(StreamIdPrefixAttribute), false);
            if (streamIdAttributes.IsEmpty())
            {
                throw new InvalidOperationException(string.Format("No StreamIdPrefixAttribute on type {0}", aggregateRootType.FullName));
            }

            return ((StreamIdPrefixAttribute)streamIdAttributes.Single()).Prefix;
        }
    }
}
