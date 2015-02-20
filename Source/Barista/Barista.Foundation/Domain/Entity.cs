using System;

namespace Barista.Foundation.Domain
{
    /// <summary>
    /// Represents an entity as defined in Domain Driven Design where the identity is of type <see cref="Guid"/>.
    /// </summary>
    public abstract class Entity : IEntity
    {
        public virtual Guid Id { get; set; }

        Guid IEntity.Id
        {
            get { return Id; }
        }
    }
}
