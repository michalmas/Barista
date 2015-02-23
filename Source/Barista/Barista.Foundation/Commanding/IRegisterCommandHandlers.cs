using Barista.Foundation.DataAccess;

namespace Barista.Foundation.Commanding
{
    /// <summary>
    /// Marker interface that indicates that the derriving clas registers command handlers. This class
    /// will automatically be called during application startup.
    /// </summary>
    /// <typeparam name="TUnitOfWork"></typeparam>
    public interface IRegisterCommandHandlers<TUnitOfWork>
        where TUnitOfWork : DomainUnitOfWork
    {
        /// <summary>
        /// Called during application startup and used to register the handlers.
        /// </summary>
        /// <param name="handlers">The registry to add the handlers to.</param>
        void Register(IHandlerRegistry<TUnitOfWork> handlers);
    }
}
