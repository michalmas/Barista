using System;
using System.Collections.Generic;
using System.Linq;

namespace Barista.Foundation.Common.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// This method executes the func a for every batch with specified size of the input.
        /// </summary>
        /// <typeparam name="TInput">The type that is accepted (IEnumerable) by the action and this method.</typeparam>
        /// <param name="input">The actual input items.</param>
        /// <param name="action">The action to execute.</param>
        /// <param name="batchSize">The number of items to call the action with.</param>
        /// <returns>The combined result of all the small batched executed.</returns>
        public static void Batch<TInput>(this IEnumerable<TInput> input, Action<IEnumerable<TInput>> action, int batchSize)
        {
            if (batchSize == 0) throw new ArgumentException("batchSize can't be 0");
            if (batchSize < 0) throw new ArgumentException("batchSize can't be negative");

            var cntr = 0;

            while (cntr < input.Count())
            {
                var batchItems = input.Skip(cntr).Take(batchSize);
                if (batchItems.Any())
                {
                    action(batchItems);
                }
                cntr += batchItems.Count();
            }
        }
    }
}
