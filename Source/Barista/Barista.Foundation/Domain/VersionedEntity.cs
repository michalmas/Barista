namespace Barista.Foundation.Domain
{
    public abstract class VersionedEntity : Entity, IVersionedEntity
    {
        public const long IgnoredVersion = -1;
        public const long NewVersion = 0;

        private long version;

        public virtual long Version
        {
            get { return version; }
            set { version = value; }
        }
    }
}
