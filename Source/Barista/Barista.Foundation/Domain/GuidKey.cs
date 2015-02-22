using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Barista.Foundation.Domain
{
    /// <summary>
    /// Represents a key that uniquely identifies an aggregate root which is uniquely identified by a textual code.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{Key}")]
    public abstract class GuidKey : IAggregateRootKey<Guid>, IXmlSerializable, ISerializable, ICloneable, IComparable
    {
        private Guid key;

        protected GuidKey()
        {
        }

        protected GuidKey(SerializationInfo info, StreamingContext context)
        {
            key = Guid.Parse(info.GetString("Key"));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GuidKey" /> struct.
        /// </summary>
        /// <param name="key">The key.</param>
        protected GuidKey(Guid key)
        {
            if (key == Guid.Empty)
            {
                throw new ArgumentException("The key of a GuidKey must have a value.");
            }

            this.key = key;
        }

        public Guid Key
        {
            get { return key; }
            protected set { key = value; }
        }

        Guid IAggregateRootKey<Guid>.Key
        {
            get { return key; }
            set { key = value; }
        }

        #region Equality members

        protected bool Equals(GuidKey other)
        {
            return Equals(key, other.key);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((GuidKey)obj);
        }

        public int CompareTo(object obj)
        {
            var other = obj as GuidKey;
            return other == null ? -1 : key.CompareTo(other.key);
        }

        public override int GetHashCode()
        {
            return (key != null ? key.GetHashCode() : 0);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Key", Key.ToString());
        }

        public static bool operator ==(GuidKey left, GuidKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GuidKey left, GuidKey right)
        {
            return !Equals(left, right);
        }

        #endregion

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public abstract object Clone();

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing a fully qualified type name.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return Key.ToString();
        }

        /// <summary>
        /// This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/> to the class.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Xml.Schema.XmlSchema"/> that describes the XML representation of the object that is produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> method and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> method.
        /// </returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Converts the GuidKey to a Guid.
        /// </summary>
        /// <returns>A guid.</returns>
        public Guid ToGuid()
        {
            return key;
        }

        /// <summary>
        /// Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized. </param>
        public void ReadXml(XmlReader reader)
        {
            key = Guid.Parse(reader.ReadElementContentAsString());
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized. </param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteValue(key.ToString());
        }
    }
}
