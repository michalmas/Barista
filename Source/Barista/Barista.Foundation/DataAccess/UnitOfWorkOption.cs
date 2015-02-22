namespace Barista.Foundation.DataAccess
{
    /// <summary>
    /// Determines how a <see cref="UnitOfWork"/> is created.
    /// </summary>
    public enum UnitOfWorkOption
    {
        /// <summary>
        /// If another unit of work already exist on the current thread then no new unit of work is created.
        /// </summary>
        ExistingOrNew,

        /// <summary>
        /// A new unit of work is created, regardless of any other existing unit of work.
        /// </summary>
        CreateNew
    }
}
