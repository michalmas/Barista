using System;
using System.Reflection;

using Barista.Foundation.Common.Exceptions;
using Barista.Foundation.Common.Extensions;
using Barista.Foundation.Domain.Events;

namespace Barista.Foundation.EventSourcing
{
    /// <summary>
    /// Represents the task of denormalizing a single event using the appropriate implementation of 
    /// <see cref="IDenormalize{TEvent}"/> on a particular denormalizer.
    /// </summary>
    public class DenormalizationCommand
    {
        public object Denormalizer { get; set; }
        public EventMessage Event { get; set; }
        public Type EventType { get; set; }

        private MethodInfo Method
        {
            get
            {
                string methodName = StaticReflection
                    .GetMemberName<IDenormalize<IDomainEvent>>(o => o.Handle(null, null));
                return Denormalizer.GetType().GetMethod(methodName, new[] { EventType, typeof(EventMetadata) });
            }
        }

        public void Execute(DispatchContext context)
        {
            try
            {
                Method.Invoke(Denormalizer, new[] { Event.Body, GetMetadata(context) });
            }
            catch (Exception exception)
            {
                throw exception.Unwrap();
            }
        }

        private EventMetadata GetMetadata(DispatchContext context)
        {
            return new EventMetadata
            {
                TimeStampUtc = context.TimeStampUtc,
                IsRedispatch = context.IsRedispatch
            };
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}({2})", Denormalizer.GetType().Name, Method.Name, Event.Body.GetType().Name);
        }
    }
}
