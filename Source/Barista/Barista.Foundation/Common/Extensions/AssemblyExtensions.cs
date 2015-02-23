using System;
using System.Linq;
using System.Reflection;

namespace Barista.Foundation.Common.Extensions
{
    public static class AssemblyExtensions
    {
        public static Type[] GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null).ToArray();
            }
        }

        /// <summary>
        /// Determines the informational version number of an assembly.
        /// </summary>
        public static string GetInformationalVersion(this Assembly assembly)
        {
            object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), inherit: false);
            return attributes.Any() ? ((AssemblyInformationalVersionAttribute)attributes.First()).InformationalVersion : "";
        }
    }
}
