using System;
using System.Collections.Generic;

using Autofac;

namespace Barista.Foundation.EventSourcing
{
    public class DenormalizerProvider : IDenormalizerProvider
    {
        private readonly ILifetimeScope lifetimeScope;

        public DenormalizerProvider(ILifetimeScope scope)
        {
            lifetimeScope = scope;
        }

        public IEnumerable<object> Get(Type eventType)
        {
            Type denormalizerType = typeof(IDenormalize<>).MakeGenericType(eventType);
            Type enumerableType = typeof(IEnumerable<>).MakeGenericType(denormalizerType);

            return (IEnumerable<object>)lifetimeScope.Resolve(enumerableType);
        }
    }
}
