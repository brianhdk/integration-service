using System;

namespace Vertica.Integration.Infrastructure.Database.Dapper
{
	public abstract class Connection
	{
		private readonly string _connectionStringName;

	    protected Connection(string connectionStringName)
		{
			_connectionStringName = connectionStringName;
		}

		internal string ConnectionStringName
		{
			get { return _connectionStringName; }
		}

        internal string DbConnectionName
        {
            get { return String.Format("Dapper.DbConnection.{0}", ConnectionStringName); }
        }

        internal string SessionName
        {
            get { return String.Format("Dapper.Session.{0}", ConnectionStringName); }
        }

        internal string SelectorName
        {
            get { return String.Format("Dapper.Selector.{0}", ConnectionStringName); }
        }

        internal string FactoryName
        {
            get { return String.Format("Dapper.Factory.{0}", ConnectionStringName); }
        }
	}
}
