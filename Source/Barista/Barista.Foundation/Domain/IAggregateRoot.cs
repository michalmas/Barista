using Barista.Foundation.DataAccess;

namespace Barista.Foundation.Domain
{
    /// <summary>
    /// Represents a domain entity that serves as the entry point of an aggregate of entities
    /// </summary>
    public interface IAggregateRoot : IEntity, IPersistable
    {
    }
}
