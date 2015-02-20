namespace Barista.Foundation.Domain
{
    public interface IVersionedEntity
    {
        long Version { get; }
    }
}
