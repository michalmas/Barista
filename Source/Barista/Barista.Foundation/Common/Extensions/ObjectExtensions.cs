using System.Collections.Generic;

namespace Barista.Foundation.Common.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        ///   Wraps an existing object into a collection.
        /// </summary>
        public static IEnumerable<T> ToEnumerable<T>(this T t)
        {
            return Equals(t, default(T)) ? new T[0] : new[] { t };
        }
    }
}
