namespace Barista.Foundation.Commanding
{
    public abstract class VersionedCommand<TIdentity> : AggregateRootCommand<TIdentity>, IVersionedCommand
    {
        protected VersionedCommand()
        {
        }

        protected VersionedCommand(TIdentity identity, long version)
            : base(identity)
        {
            Version = version;
        }

        public long Version { get; set; }
    }
}
