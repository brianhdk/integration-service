using System;

namespace Vertica.Integration.Infrastructure.Database.Dapper
{
	public abstract class Connection
	{
		private readonly ConnectionString _connectionString;
	    private readonly string _postfix;

	    protected Connection(ConnectionString connectionString)
		{
	        if (connectionString == null) throw new ArgumentNullException("connectionString");

	        _connectionString = connectionString;
	        _postfix = String.Format("{0}.{1}", GetType().Name, GetType().GetHashCode());
		}

	    internal ConnectionString ConnectionStringInternal
		{
            get { return _connectionString; }
		}

        internal string DbConnectionName
        {
            get { return String.Format("Dapper.DbConnection.{0}", _postfix); }
        }

        internal string SessionName
        {
            get { return String.Format("Dapper.Session.{0}", _postfix); }
        }

        internal string SelectorName
        {
            get { return String.Format("Dapper.Selector.{0}", _postfix); }
        }

        internal string FactoryName
        {
            get { return String.Format("Dapper.Factory.{0}", _postfix); }
        }
	}
}
