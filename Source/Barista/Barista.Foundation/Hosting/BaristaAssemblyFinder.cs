using System;
using System.Linq;
using System.Reflection;

namespace Barista.Foundation.Hosting
{
    /// <summary>
    /// Finds all Barista assemblies.
    /// </summary>
    public class BaristaAssemblyFinder : IBaristaAssemblyFinder
    {
        private const string Barista = "Barista";
        private const string SpecsSuffix = "Specs";

        /// <summary>
        /// Gets all loaded assemblies created by Barista.
        /// </summary>
        /// <remarks>Skips specs assemblies.</remarks>
        public Assembly[] GetAll()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a =>
                    GetCompanyName(a) == Barista &&
                    IsProductionAssembly(a))
                .ToArray();
        }

        /// <summary>
        /// Gets the name of the company.
        /// </summary>
        private static string GetCompanyName(Assembly assembly)
        {
            object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            if (attributes.Length == 0)
            {
                return string.Empty;
            }

            return ((AssemblyCompanyAttribute)attributes[0]).Company;
        }

        /// <summary>
        /// Specifies whether the <param name="assembly" /> is used in a production environment.
        /// </summary>
        private static bool IsProductionAssembly(Assembly assembly)
        {
            return !assembly.GetName().Name.EndsWith("." + SpecsSuffix);
        }
    }
}
