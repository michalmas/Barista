using System;
using System.Collections.Concurrent;

namespace Barista.Foundation.Common.Extensions
{
    /// <summary>
    /// Extension methods for Func types.
    /// </summary>
    public static class FuncExtensions
    {
        /// <summary>
        /// Returns a memoized version of a referentially transparent function. The memoized version of the 
        /// function keeps a cache of the mapping from arguments to results and, when calls with the same 
        /// arguments are repeated often, has higher performance at the expense of higher memory use.
        /// </summary>
        /// <example>
        ///     var inc = x => x + 1;
        ///     var memoizedInc = inc.Memoize();
        ///     Console.WriteLine(memoizedInc(1));  // Calls inc and caches the result
        ///     Console.WriteLine(memoizedInc(1));  // Reads the result from the cache
        /// </example>
        public static Func<T, TResult> Memoize<T, TResult>(this Func<T, TResult> func)
        {
            var dictionary = new ConcurrentDictionary<T, TResult>();
            return arg => dictionary.GetOrAdd(arg, func);
        }

        /// <summary>
        /// Returns a memoized version of a referentially transparent function. The memoized version of the 
        /// function keeps a cache of the mapping from arguments to results and, when calls with the same 
        /// arguments are repeated often, has higher performance at the expense of higher memory use.
        /// </summary>
        /// <example>
        ///     var add = (x, y) => x + y;
        ///     var memoizedAdd = add.Memoize();
        ///     Console.WriteLine(memoizedAdd(1, 2));  // Calls add and caches the result
        ///     Console.WriteLine(memoizedAdd(1, 2));  // Reads the result from the cache
        /// </example>
        public static Func<T1, T2, TResult> Memoize<T1, T2, TResult>(this Func<T1, T2, TResult> f)
        {
            return f.Tuplify().Memoize().Detuplify();
        }

        private static Func<Tuple<T1, T2>, TResult> Tuplify<T1, T2, TResult>(this Func<T1, T2, TResult> func)
        {
            return t => func(t.Item1, t.Item2);
        }

        private static Func<T1, T2, TResult> Detuplify<T1, T2, TResult>(this Func<Tuple<T1, T2>, TResult> func)
        {
            return (arg1, arg2) => func(Tuple.Create(arg1, arg2));
        }
    }
}
