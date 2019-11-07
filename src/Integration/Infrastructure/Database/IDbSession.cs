using System;
using System.Collections.Generic;
using System.Data;
using Dapper;

namespace Vertica.Integration.Infrastructure.Database
{
    public interface IDbSession : IDisposable
    {
        IDbTransaction BeginTransaction(IsolationLevel? isolationLevel = null);

        int Execute(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null);
        T ExecuteScalar<T>(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null);

	    IEnumerable<dynamic> Query(string sql, dynamic param = null);
        IEnumerable<T> Query<T>(string sql, dynamic param = null);

		IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id");
		IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = "Id");
		IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null, string splitOn = "Id");

		SqlMapper.GridReader QueryMultiple(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null);

        IDbConnection Connection { get; }
        IDbTransaction CurrentTransaction { get; }
    }
}