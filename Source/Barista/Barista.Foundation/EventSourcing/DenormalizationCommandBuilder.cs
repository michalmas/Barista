using System;
using System.Collections.Generic;

using Barista.Foundation.Domain.Events;

namespace Barista.Foundation.EventSourcing
{
    internal class DenormalizationCommandBuilder
    {
        public static IEnumerable<DenormalizationCommand> BuildsCommandsFor(EventMessage @event,
            Func<Type, IEnumerable<object>> getDenormalizers)
        {
            Type eventType = @event.Body.GetType();

            do
            {
                var denormalizers = getDenormalizers(eventType);

                foreach (object denormalizer in denormalizers)
                {
                    yield return new DenormalizationCommand
                    {
                        Denormalizer = denormalizer,
                        Event = @event,
                        EventType = eventType
                    };
                }

                eventType = eventType.BaseType;
            }
            while ((eventType != null) && (typeof(IDomainEvent).IsAssignableFrom(eventType)));
        }
    }
}
