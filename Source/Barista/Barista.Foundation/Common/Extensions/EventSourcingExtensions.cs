using System;

using Barista.Foundation.DataAccess;
using Barista.Foundation.Domain;

namespace Barista.Foundation.Common.Extensions
{
    public static class EventSourcingExtensions
    {
        /// <summary>
        /// Generates a deterministic <see cref="Guid"/> based on the stream prefix of an aggregate root.
        /// </summary>
        public static Guid ToDeterministicGuid(this object value, Type aggregateRootType)
        {
            string streamPrefix = PersistableObjectReflector.GetTypeStreamPrefix(aggregateRootType);
            return ToDeterministicGuid(value, streamPrefix);
        }

        /// <summary>
        /// Generates a deterministic <see cref="Guid"/> based on a string.
        /// </summary>
        public static Guid ToDeterministicGuid(this object value, string prefix)
        {
            Guid result;

            object keyValue = IsAggregateRootKey(value) ? GetValueFromAggregateRootKey(value) : value;

            if (keyValue is Guid)
            {
                result = (Guid)keyValue;
            }
            else if (keyValue is GuidKey)
            {
                result = ((GuidKey)keyValue).Key;
            }
            /*else if (keyValue is string)
            {
                result = GuidUtility.Create(GuidUtility.VisionSuiteNamespace,
                    string.Format("{0}_{1}", prefix, keyValue));
            }
            else if (keyValue is Descriptor)
            {
                string descriptorCode = ((Descriptor)keyValue).Code;
                result = GuidUtility.Create(GuidUtility.VisionSuiteNamespace,
                    string.Format("{0}_{1}", prefix, descriptorCode));
            }*/
            else
            {
                throw new ArgumentException("Cannot generate a deterministic Guid from a " + keyValue.GetType().Name);
            }

            return result;
        }

        private static bool IsAggregateRootKey(object value)
        {
            return (value is IAggregateRootKey<Guid>) || (value is IAggregateRootKey<string>);
        }

        private static object GetValueFromAggregateRootKey(object value)
        {
            var guidAggregateRootKey = value as IAggregateRootKey<Guid>;
            if (guidAggregateRootKey != null)
            {
                return guidAggregateRootKey.Key;
            }

            var stringAggregateRootKey = value as IAggregateRootKey<string>;
            if (stringAggregateRootKey != null)
            {
                return stringAggregateRootKey.Key;
            }

            return value;
        }
    }
}
