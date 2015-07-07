using System;
using System.Data.SqlClient;
using Vertica.Integration.Infrastructure.Database.Databases;

namespace Vertica.Integration.Infrastructure.Database.Extensions
{
    internal static class DbExtensions
    {
        /// <summary>
        /// Wraps the session and catches SqlException providing the client a better error message with actions on how to resolve it.
        /// </summary>
        public static T Wrap<T>(this IDbSession session, Func<IDbSession, T> action)
        {
            if (session == null) throw new ArgumentNullException("session");
            if (action == null) throw new ArgumentNullException("action");

            try
            {
                return action(session);
            }
            catch (SqlException ex)
            {
                throw new IntegrationDbException(ex);
            }
        }
    }
}