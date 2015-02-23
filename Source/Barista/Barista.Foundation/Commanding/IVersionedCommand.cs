namespace Barista.Foundation.Commanding
{
    public interface IVersionedCommand : IAggregateRootCommand
    {
        long Version { get; set; }
    }
}
