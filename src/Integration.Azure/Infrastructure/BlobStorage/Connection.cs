using System;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Azure.Infrastructure.BlobStorage
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

        internal string CloudBlobClient
        {
            get { return String.Format("AzureBlobStorage.Session.{0}", _postfix); }
        }

        internal string SelectorName
        {
            get { return String.Format("AzureBlobStorage.Selector.{0}", _postfix); }
        }

        internal string FactoryName
        {
            get { return String.Format("AzureBlobStorage.Factory.{0}", _postfix); }
        }
	}
}
