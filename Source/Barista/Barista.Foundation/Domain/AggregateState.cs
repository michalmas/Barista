using System;
using System.Reflection;
using Barista.Foundation.Common.Exceptions;
using Barista.Foundation.Common.Extensions;
using Barista.Foundation.Domain.Events;

namespace Barista.Foundation.Domain
{
    /// <summary>
    /// Represents the state of an aggregate based on <see cref="IEventSource"/> and processes both current and historical
    /// events.
    /// </summary>
    public abstract class AggregateState
    {
        /// <summary>
        /// Applies the specified <see cref="IDomainEvent"/> to the current state by invoking the appropriate When method
        /// that takes a particular event.
        /// </summary>
        public void Process(IDomainEvent @event)
        {
            if (!@event.GetType().HasAttribute<ObsoleteAttribute>())
            {
                MethodInfo info = EventMappingCache.GetHandlerFor(GetType(), @event.GetType());

                try
                {
                    info.Invoke(this, new[] { @event });
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.Unwrap();
                }
            }
        }
    }
}
