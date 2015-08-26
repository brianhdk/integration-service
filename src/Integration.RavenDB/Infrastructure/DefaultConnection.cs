﻿using System;
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
			if (connection == null) throw new ArgumentNullException("connection");

			_connection = connection;
		}

		protected internal override void Initialize(IDocumentStore documentStore)
		{
			if (_connection != null)
			{
				_connection.Initialize(documentStore);
			}
			else
			{
				base.Initialize(documentStore);
			}
		}

		protected internal override IDocumentStore Create()
		{
			if (_connection != null)
				return _connection.Create();

			return base.Create();
		}
	}
}