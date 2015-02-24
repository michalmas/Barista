using System;

using Barista.Foundation.DataAccess;
using Barista.Foundation.Domain;
using Barista.Foundation.EventSourcing;

namespace Barista.Foundation.Denormalization
{
    /// <summary>
    /// Defines the basic plumbing necessary for denormalizing events while honoring versions and redispatching
    /// the event store. Delegates the algorithm for finding an existing projection to a concrete implementation.
    /// </summary>
    public abstract class Denormalizer<TProjection, TIdentity> 
        where TProjection : class, IQueryModel, new()
    {
        protected Denormalizer(Func<ProjectionsUnitOfWork> uowFactory)
        {
            UowFactory = uowFactory;
        }

        protected Func<ProjectionsUnitOfWork> UowFactory { get; private set; }

        /// <summary>
        /// Finds or creates a projection identified by the provided <paramref name="identity"/> and passes it
        /// to a user-provided <paramref name="action"/>. 
        /// </summary>
        /// <remarks>
        /// Ensures that the action is only executed if the <paramref name="version"/> exceeds the version of 
        /// the projection, or if the <paramref name="metadata"/> indicates a redispatch is taking place.
        /// </remarks>
        protected void OnHandle(TIdentity identity, long version, EventMetadata metadata, Action<TProjection> action,
            MissingProjectionBehavior missingProjectionBehavior = MissingProjectionBehavior.Create)
        {
            if (Equals(identity, default(TIdentity)))
            {
                throw new InvalidOperationException("Don't know which projection to denormalize, missing its identity.");
            }

            using (ProjectionsUnitOfWork uow = UowFactory())
            {
                Repository<TProjection> projections = uow.GetRepository<TProjection>();
                TProjection projection = Find(identity, projections);
                if (projection == null)
                {
                    if (missingProjectionBehavior == MissingProjectionBehavior.Create)
                    {
                        projection = CreateNewProjection(version, action);
                        InitializeProjection(projection, identity);
                        projections.Add(projection);
                    }
                }
                else
                {
                    UpdateProjection(version, action, metadata, projection);
                }
            }
        }

        /// <summary>
        ///   Used to initialize a projection after it has been created.
        /// </summary>
        /// <param name="projection"></param>
        /// <param name="identity"></param>
        protected abstract void InitializeProjection(TProjection projection, TIdentity identity);

        private TProjection CreateNewProjection(long version, Action<TProjection> action)
        {
            var projection = new TProjection();

            var versionedProjection = projection as IHaveVersion;
            if (versionedProjection != null)
            {
                versionedProjection.Version = version;
            }

            action(projection);
            return projection;
        }

        /// <summary>
        /// Finds an existing projection identified by the provided <paramref name="identity"/> and removes it. 
        /// </summary>
        /// <remarks>
        /// Ensures that the projection is only removed if the <paramref name="version"/> exceeds the version of 
        /// the projection, or if the <paramref name="metadata"/> indicates a redispatch is taking place.
        /// </remarks>
        protected void RemoveProjection(TIdentity identity, long version, EventMetadata metadata)
        {
            using (ProjectionsUnitOfWork uow = UowFactory())
            {
                Repository<TProjection> projections = uow.GetRepository<TProjection>();
                var projection = Find(identity, projections);
                if (projection != null)
                {
                    UpdateProjection(version, projections.Remove, metadata, projection);
                }
            }
        }

        /// <summary>
        /// Defines a contract for an algorithm to find an existing projection based on the 
        /// provided <paramref name="identity"/>.
        /// </summary>
        /// <returns>
        /// Should return the existing projection, or <c>null</c> if no projection for that identity exists (anymore).
        /// </returns>
        protected abstract TProjection Find(TIdentity identity, Repository<TProjection> projections);

        private void UpdateProjection(long version, Action<TProjection> action, EventMetadata metadata,
            TProjection projection)
        {
            var versionedProjection = projection as IHaveVersion;
            if (versionedProjection != null && version != VersionedEntity.IgnoredVersion)
            {
                if (metadata.IsRedispatch || (version > versionedProjection.Version))
                {
                    long maxVersion = Math.Max(versionedProjection.Version, version);
                    action(projection);
                    versionedProjection.Version = maxVersion;
                }
            }
            else
            {
                action(projection);
            }
        }
    }
}
