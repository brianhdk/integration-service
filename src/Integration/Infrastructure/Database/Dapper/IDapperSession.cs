using System;
using System.Collections.Generic;
using System.Data;

namespace Vertica.Integration.Infrastructure.Database.Dapper
{
    public interface IDapperSession : IDisposable
    {
        IDbTransaction BeginTransaction(IsolationLevel? isolationLevel = null);

        int Execute(string sql, dynamic param = null);
        T ExecuteScalar<T>(string sql, dynamic param = null);
        IEnumerable<T> Query<T>(string sql, dynamic param = null);

        IDbConnection Connection { get; }
    }
}