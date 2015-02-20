using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Barista.Foundation.Common.Exceptions
{
    /// <summary>
    /// Represents a technical application exception which display message should be resolved through its <see cref="Error" /> property.
    /// </summary>
    public class ApplicationErrorException : Exception, IEnumerable
    {
        /// <summary>
        /// The error that identifies the reason for this exception.
        /// </summary>
        public Error Error { get; private set; }

        /// <summary>
        /// The parameters associated with the current <see cref="ApplicationErrorException" />.
        /// </summary>
        public IDictionary<string, object> Parameters { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationErrorException"/> class.
        /// </summary>
        /// <param name="error">The <see cref="Error"/> that describes the exception.</param>
        public ApplicationErrorException(Error error)
            : this(error, null, new Dictionary<string, object>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationErrorException"/> class.
        /// </summary>
        /// <param name="error">The <see cref="Error"/> that describes the exception.</param>
        /// <param name="parameters">The parameters for the exception.</param>
        public ApplicationErrorException(Error error, IDictionary<string, object> parameters)
            : this(error, null, parameters)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationErrorException"/> class.
        /// </summary>
        /// <param name="error">The <see cref="Error"/> that describes the exception.</param>
        /// <param name="innerException">The inner exception.</param>
        public ApplicationErrorException(Error error, Exception innerException)
            : this(error, innerException, new Dictionary<string, object>())
        {
        }

        private ApplicationErrorException(Error error, Exception innerException, IDictionary<string, object> parameters)
            : base(error.Code, innerException)
        {
            Error = error;
            Parameters = parameters;
        }

        protected ApplicationErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        /// <summary>
        /// Adds a named parameter to the first violation of this exception object.
        /// </summary>
        public void Add(string key, object value)
        {
            Parameters.Add(key, value);
        }

        public IEnumerator GetEnumerator()
        {
            return Parameters.GetEnumerator();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(base.ToString());
            sb.AppendLine("Parameters:");

            foreach (KeyValuePair<string, object> parameter in Parameters)
            {
                sb.AppendLine(string.Format("Name: {0}, Value: {1}", parameter.Key, parameter.Value));
            }

            return sb.ToString();
        }
    }
}
