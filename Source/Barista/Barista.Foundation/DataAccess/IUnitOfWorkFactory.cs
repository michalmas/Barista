namespace Barista.Foundation.DataAccess
{
    public interface IUnitOfWorkFactory<out TUnitOfWork> 
        where TUnitOfWork : UnitOfWork
    {
        /// <summary>
        /// Starts or continues a new unit-of-work that tracks changes made the entities obtained through the associated repositories.
        /// </summary>
        /// <returns>
        /// If the current thread is already associated with a unit-of-work, then the same unit-of-work is returned. Otherwise a new unit-of-work
        /// is created.
        /// </returns>
        TUnitOfWork Create();

        /// <summary>
        /// Starts or continues a new unit-of-work that tracks changes made the entities obtained through the associated repositories.
        /// </summary>
        /// <paramref name="option">
        /// Determines whether the method should try to enlist in an existing unit of work or always create a new one.
        /// </paramref>
        TUnitOfWork Create(UnitOfWorkOption option);
    }
}
