using System;

namespace Barista.Foundation.EventSourcing
{
    /// <summary>
    /// Used to specify the prefix that is used to generate the stream id for an event sourced aggregate.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class StreamIdPrefixAttribute : Attribute
    {
        public StreamIdPrefixAttribute(string prefix)
        {
            Prefix = prefix;
        }

        /// <summary>
        /// Gets the prefix to be used to generate the stream id for an event sourced aggregate.
        /// </summary>
        public string Prefix { get; private set; }
    }
}
