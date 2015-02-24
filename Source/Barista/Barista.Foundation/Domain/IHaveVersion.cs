namespace Barista.Foundation.Domain
{
    public interface IHaveVersion
    {
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        long Version { get; set; }
    }
}
