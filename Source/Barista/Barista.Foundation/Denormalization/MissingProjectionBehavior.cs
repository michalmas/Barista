namespace Barista.Foundation.Denormalization
{
    /// <summary>
    /// Enum for identifying what to do when the projection can't be found when denormalizing an event.
    /// </summary>
    public enum MissingProjectionBehavior
    {
        Create,
        Ignore
    }
}
