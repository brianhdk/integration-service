using System;
using Raven.Client;
using Vertica.Integration.RavenDB.Infrastructure;

namespace Vertica.Integration.RavenDB
{
	internal class RavenDbFactory<TConnection> : IRavenDbFactory<TConnection>, IDisposable
		where TConnection : Connection
	{
		private readonly Lazy<IDocumentStore> _documentStore;

		public RavenDbFactory(Connection connection)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			
			_documentStore = new Lazy<IDocumentStore>(() =>
			{
				IDocumentStore documentStore = connection.Create();
				connection.Initialize(documentStore);

				return documentStore;
			});
		}

		public IDocumentStore DocumentStore
		{
			get { return _documentStore.Value; }
		}

		public void Dispose()
		{
			if (_documentStore.IsValueCreated)
				_documentStore.Value.Dispose();
		}
	}

	internal class RavenDbFactory : IRavenDbFactory
	{
		private readonly IRavenDbFactory<DefaultConnection> _decoree;

		public RavenDbFactory(IRavenDbFactory<DefaultConnection> decoree)
		{
			if (decoree == null) throw new ArgumentNullException("decoree");

			_decoree = decoree;
		}

		public IDocumentStore DocumentStore
		{
			get { return _decoree.DocumentStore; }
		}
	}
}