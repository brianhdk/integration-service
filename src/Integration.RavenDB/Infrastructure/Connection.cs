using System;
using Castle.MicroKernel;
using Raven.Client;
using Raven.Client.Document;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.RavenDB.Infrastructure
{
	public abstract class Connection
	{
		protected Connection(ConnectionString connectionString)
		{
			if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

			ConnectionString = connectionString;
		}

		protected internal ConnectionString ConnectionString { get; }

		protected internal virtual void Initialize(IDocumentStore documentStore, IKernel kernel)
		{
			if (documentStore == null) throw new ArgumentNullException(nameof(documentStore));
			if (kernel == null) throw new ArgumentNullException(nameof(kernel));

			documentStore.Initialize();
		}

		protected internal virtual IDocumentStore Create(IKernel kernel)
		{
			if (kernel == null) throw new ArgumentNullException(nameof(kernel));

			var documentStore = new DocumentStore();
			documentStore.ParseConnectionString(ConnectionString);

			return documentStore;
		}
	}
}