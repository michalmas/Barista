using System.Reflection;

namespace Barista.Foundation.Hosting
{
    /// <summary>
    /// Gets all loaded assemblies created by Barista.
    /// </summary>
    /// <remarks>Skips specs assemblies.</remarks>
    public interface IBaristaAssemblyFinder
    {
        /// <summary>
        /// Gets all loaded assemblies created by Barista.
        /// </summary>
        /// <remarks>Skips specs assemblies.</remarks>
        Assembly[] GetAll();
    }
}
