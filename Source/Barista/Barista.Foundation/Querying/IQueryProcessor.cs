namespace Barista.Foundation.Querying
{
    /// <summary>
    /// Represents a generic access point for executing (cross-product) queries.
    /// </summary>
    public interface IQueryProcessor
    {
        /// <summary>
        /// Executes the specified query by finding an appropriate handler and passing the query to it.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        TResult Execute<TResult>(IQuery<TResult> query);
    }
}
