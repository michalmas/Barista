namespace Barista.Foundation.Querying
{
    /// <summary>
    /// Defines the contract for a class handling a specific query.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResult">The type of the result of the query.</typeparam>
    public interface IQueryHandler<in TQuery, out TResult>
    {
        /// <summary>
        /// Handles the supplied query and returns the results.
        /// </summary>
        TResult Handle(TQuery query);
    }
}
