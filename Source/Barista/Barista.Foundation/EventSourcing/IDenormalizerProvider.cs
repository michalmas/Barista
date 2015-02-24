using System;
using System.Collections.Generic;

namespace Barista.Foundation.EventSourcing
{
    /// <summary>
    /// Provider class that provides denormalizers based on the specified <see cref="DenormalizerSelection"/>.
    /// </summary>
    public interface IDenormalizerProvider
    {
        /// <summary>
        /// Gets the denormalizers that denormalize the specified type, based on the specified <see cref="DenormalizerSelection"/>.
        /// </summary>
        IEnumerable<object> Get(Type eventType);
    }
}
