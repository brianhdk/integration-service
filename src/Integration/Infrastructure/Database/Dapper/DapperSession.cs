using System;
using System.Collections.Generic;
using System.Data;
using Dapper;

namespace Vertica.Integration.Infrastructure.Database.Dapper
{
    internal class DapperSession : IDapperSession
    {
        private readonly IDbConnection _connection;
        private readonly Stack<IDbTransaction> _transactions;

        public DapperSession(IDbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            _connection = connection;
            _connection.Open();

            _transactions = new Stack<IDbTransaction>(capacity: 3);
        }

        public IDbTransaction BeginTransaction(IsolationLevel? isolationLevel = null)
        {
            IDbTransaction transaction = isolationLevel.HasValue
                ? _connection.BeginTransaction(isolationLevel.Value)
                : _connection.BeginTransaction();

            _transactions.Push(transaction);

            return new TransactionScope(transaction, () => _transactions.Pop());
        }

        public int Execute(string sql, dynamic param = null)
        {
            return SqlMapper.Execute(_connection, sql, param, CurrentTransaction);
        }

        public T ExecuteScalar<T>(string sql, dynamic param = null)
        {
            return SqlMapper.ExecuteScalar<T>(_connection, sql, param, CurrentTransaction);
        }
        
        public IEnumerable<T> Query<T>(string sql, dynamic param = null)
        {
            return SqlMapper.Query<T>(_connection, sql, param, CurrentTransaction);
        }

        public IDbConnection Connection
        {
            get { return _connection; }
        }

        public virtual void Dispose()
        {
            _connection.Dispose();
        }

        private IDbTransaction CurrentTransaction
        {
            get
            {
                return _transactions.Count > 0 ? _transactions.Peek() : null;
            }
        }

        private class TransactionScope : IDbTransaction, IExposeTransaction
        {
            private readonly IDbTransaction _transaction;
            private readonly Action _beforeDispose;

            public TransactionScope(IDbTransaction transaction, Action beforeDispose)
            {
                _transaction = transaction;
                _beforeDispose = beforeDispose;
            }

            public void Dispose()
            {
                _beforeDispose();
                _transaction.Dispose();
            }

            public void Commit()
            {
                _transaction.Commit();
            }

            public void Rollback()
            {
                _transaction.Rollback();
            }

            public IDbConnection Connection
            {
                get { return _transaction.Connection; }
            }

            public IsolationLevel IsolationLevel
            {
                get { return _transaction.IsolationLevel; }
            }

            public IDbTransaction Transaction
            {
                get { return _transaction; }
            }
        }
    }
}