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

		protected internal ConnectionString ConnectionString { get; private set; }

		protected internal virtual void Initialize(IDocumentStore documentStore)
		{
			documentStore.Initialize();
		}

		protected internal virtual IDocumentStore Create()
		{
			var documentStore = new DocumentStore();
			documentStore.ParseConnectionString(ConnectionString);

			return documentStore;
		}
	}
}