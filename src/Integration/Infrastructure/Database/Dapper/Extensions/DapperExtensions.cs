using System;
using System.Data.SqlClient;
using Vertica.Integration.Infrastructure.Database.Dapper.Databases;

namespace Vertica.Integration.Infrastructure.Database.Dapper.Extensions
{
    internal static class DapperExtensions
    {
        /// <summary>
        /// Wraps the session and catches SqlException providing the client a better error message with actions on how to resolve it.
        /// </summary>
        public static T Wrap<T>(this IDapperSession session, Func<IDapperSession, T> action)
        {
            if (session == null) throw new ArgumentNullException("session");
            if (action == null) throw new ArgumentNullException("action");

            try
            {
                return action(session);
            }
            catch (SqlException ex)
            {
                throw new IntegrationDbException(String.Format(@"{0}

Make sure the database is created, that it is functional and up-to-date with the latest migrations. 

Try run the ""MigrateTask"" to ensure it's running with the latest database schema.

If this error continues, check your connection string and permissions to the database.", ex.Message), ex);
            }
        }
    }
}