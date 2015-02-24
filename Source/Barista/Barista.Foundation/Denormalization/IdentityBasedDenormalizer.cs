using System;

using Barista.Foundation.DataAccess;
using Barista.Foundation.Domain;

namespace Barista.Foundation.Denormalization
{
    /// <summary>
    /// Denormalizer that supports projections which identity is directly mapped as the primary key.
    /// </summary>
    public abstract class IdentityBasedDenormalizer<TProjection, TIdentity> : Denormalizer<TProjection, TIdentity>
        where TProjection : class, IQueryModel, new()
    {
        protected IdentityBasedDenormalizer(Func<ProjectionsUnitOfWork> uowFactory)
            : base(uowFactory)
        {

        }

        protected override TProjection Find(TIdentity identity, Repository<TProjection> projections)
        {
            return projections.Find(identity);
        }

        protected override void InitializeProjection(TProjection projection, TIdentity identity)
        {
            PersistableObjectReflector.SetKey(projection, identity);
        }
    }
}
