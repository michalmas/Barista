using System;

using NHibernate;
using NHibernate.Engine;
using NHibernate.Linq;
using NHibernate.Persister.Entity;

namespace Barista.Foundation.DataAccess.NHibernate
{
    internal static class SessionExtensions
    {
        /// <summary>
        /// Deletes all the records for a particular entity, including any additional tables within the same schema.
        /// </summary>
        /// <remarks>
        /// It will not remove any related or join tables, unless you provide the names of those tables explicitly.
        /// </remarks>
        public static void DeleteImmediately<TEntity>(this ISession session, params string[] tableNames)
        {
            ISessionImplementor sessionImplementor = session.GetSessionImplementation();

            string schemaPrefix = GetSchemaPrefix(typeof(TEntity), sessionImplementor);

            foreach (string tableName in tableNames)
            {
                session.CreateSQLQuery("delete from " + schemaPrefix + tableName).ExecuteUpdate();
            }

            session.CreateQuery("delete from " + typeof(TEntity).Name).ExecuteUpdate();
        }

        private static string GetSchemaPrefix(Type entityType, ISessionImplementor session)
        {
            string schemaQualifiedTableName = ((ILockable)session
                .GetEntityPersister(entityType.FullName, Activator.CreateInstance(entityType, true))).RootTableName;

            int schemaDeliminator = schemaQualifiedTableName.IndexOf(".");
            if (schemaDeliminator == -1)
            {
                schemaDeliminator = schemaQualifiedTableName.IndexOf("_");
            }

            string schemaPrefix = "";
            if (schemaDeliminator != -1)
            {
                schemaPrefix = schemaQualifiedTableName.Substring(0, schemaDeliminator + 1);
            }

            return schemaPrefix;
        }
    }
}
