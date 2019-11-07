using System;
using Castle.MicroKernel;
using Raven.Client;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.RavenDB.Infrastructure
{
	public sealed class DefaultConnection : Connection
	{
		private readonly Connection _connection;

		internal DefaultConnection(ConnectionString connectionString)
			: base(connectionString)
		{
		}

		internal DefaultConnection(Connection connection)
			: base(connection.ConnectionString)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_connection = connection;
		}

		protected internal override void Initialize(IDocumentStore documentStore, IKernel kernel)
		{
			if (_connection != null)
			{
				_connection.Initialize(documentStore, kernel);
			}
			else
			{
				base.Initialize(documentStore, kernel);
			}
		}

		protected internal override IDocumentStore Create(IKernel kernel)
		{
			if (_connection != null)
				return _connection.Create(kernel);

			return base.Create(kernel);
		}
	}
}