using System;
using Raven.Client;
using Vertica.Integration.RavenDB.Infrastructure;

namespace Vertica.Integration.RavenDB
{
	internal class RavenDBFactory<TConnection> : IRavenDBFactory<TConnection>, IDisposable
		where TConnection : Connection
	{
		private readonly Lazy<IDocumentStore> _documentStore;

		public RavenDBFactory(Connection connection)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			
			_documentStore = new Lazy<IDocumentStore>(connection.Create);
		}

		public IDocumentStore DocumentStore
		{
			get { return _documentStore.Value; }
		}

		public void Dispose()
		{
			Console.WriteLine("Disposing {0}", typeof (TConnection).Name);

			if (_documentStore.IsValueCreated)
				_documentStore.Value.Dispose();
		}
	}

	internal class RavenDBFactory : IRavenDBFactory
	{
		private readonly IRavenDBFactory<DefaultConnection> _decoree;

		public RavenDBFactory(IRavenDBFactory<DefaultConnection> decoree)
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