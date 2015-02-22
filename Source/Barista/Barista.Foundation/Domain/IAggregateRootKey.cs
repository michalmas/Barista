namespace Barista.Foundation.Domain
{
    /// <summary>
    /// Identifies a value object that represents a functional key to another aggregate.
    /// </summary>
    public interface IAggregateRootKey<T>
    {
        T Key { get; set; }
    }
}
