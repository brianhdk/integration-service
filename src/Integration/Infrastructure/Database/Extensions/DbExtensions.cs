using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Vertica.Integration.Infrastructure.Parsing;

namespace Vertica.Integration.Infrastructure.Database.Extensions
{
    public static class DbExtensions
    {
        /// <summary>
        /// Wraps the session and catches SqlException providing the client a better error message with actions on how to resolve it.
        /// </summary>
        internal static T Wrap<T>(this IDbSession session, Func<IDbSession, T> action)
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

        public static string QueryToCsv(this IDbSession session, string sql, dynamic param = null, Action<CsvRow.ICsvRowBuilderConfiguration> configuration = null)
        {
            if (session == null) throw new ArgumentNullException("session");
            if (String.IsNullOrWhiteSpace(sql)) throw new ArgumentException(@"Value cannot be null or empty.", "sql");

            IEnumerable<dynamic> query = SqlMapper.Query(session.Connection, sql, param, session.CurrentTransaction);

            string[] headers = null;

            return
                CsvRow.BeginRows()
                    .Configure(x =>
                    {
                        if (configuration != null)
                            configuration(x);
                        else
                            x.ReturnHeaderAsRow();
                    })
                    .From(query, data =>
                    {
                        var row = (IDictionary<string, object>)data;

                        if (headers == null)
                            headers = row.Keys.ToArray();

                        return row.Values.Select(x => x != null ? x.ToString() : null).ToArray();
                    })
                    .Headers(headers)
                    .ToString();
        }
    }
}