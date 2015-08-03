using System;
using Raven.Client;
using Raven.Client.Document;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.RavenDB.Infrastructure
{
	public abstract class Connection
	{
		protected Connection(ConnectionString connectionString)
		{
			if (connectionString == null) throw new ArgumentNullException("connectionString");

			ConnectionString = connectionString;
		}

		protected ConnectionString ConnectionString { get; private set; }

		protected internal virtual IDocumentStore Create()
		{
			IDocumentStore documentStore = CreateDocumentStore();
			documentStore.Initialize();

			return documentStore;
		}

		protected virtual IDocumentStore CreateDocumentStore()
		{
			var documentStore = new DocumentStore();
			documentStore.ParseConnectionString(ConnectionString);

			return documentStore;
		}
	}
}