using System;
using Raven.Client;

namespace Vertica.Integration.RavenDB.Infrastructure
{
	public sealed class DefaultConnection : Connection
	{
		private readonly Connection _connection;

		internal DefaultConnection(Connection connection)
			: base(connection.ConnectionString)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			_connection = connection;
		}

		protected internal override void Initialize(IDocumentStore documentStore)
		{
			_connection.Initialize(documentStore);
		}

		protected internal override IDocumentStore Create()
		{
			return _connection.Create();
		}
	}
}