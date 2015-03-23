using System;

namespace Vertica.Integration.Azure.Infrastructure.BlobStorage
{
	public abstract class Connection
	{
		private readonly string _connectionStringName;

	    protected Connection(string connectionStringName)
		{
            if (String.IsNullOrWhiteSpace(connectionStringName)) throw new ArgumentException("Value cannot be null or empty.");

			_connectionStringName = connectionStringName;
		}

	    public string ConnectionStringName
		{
			get { return _connectionStringName; }
		}

        internal string CloudBlobClient
        {
            get { return String.Format("AzureBlobStorage.Session.{0}", ConnectionStringName); }
        }

        internal string SelectorName
        {
            get { return String.Format("AzureBlobStorage.Selector.{0}", ConnectionStringName); }
        }

        internal string FactoryName
        {
            get { return String.Format("AzureBlobStorage.Factory.{0}", ConnectionStringName); }
        }
	}
}
