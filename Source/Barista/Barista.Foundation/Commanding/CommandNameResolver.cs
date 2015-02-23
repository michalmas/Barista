using System;

namespace Barista.Foundation.Commanding
{
    /// <summary>
    /// Returns the name of the command for the <see cref="Type"/>.
    /// </summary>
    public static class CommandNameResolver
    {
        /// <summary>
        /// Gets the name for the <see cref="TCommand"/>.
        /// </summary>
        public static string GetName<TCommand>()
        {
            return GetName(typeof(TCommand));
        }

        /// <summary>
        /// Gets the name for the <see cref="Type"/>.
        /// </summary>
        public static string GetName(Type commandType)
        {
            const string suffix = "Command";
            string name = commandType.Name;
            int index = name.LastIndexOf(suffix, StringComparison.Ordinal);

            if (index != -1)
            {
                name = name.Substring(0, index);
            }

            return name;
        }
    }
}
