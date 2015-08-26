﻿using System;
using MongoDB.Driver;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.MongoDB.Infrastructure
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

		protected internal override MongoUrl MongoUrl
		{
			get
			{
				if (_connection != null)
					return _connection.MongoUrl;

				return base.MongoUrl;
			}
		}

		protected internal override IMongoClient Create()
		{
			if (_connection != null)
				return _connection.Create();

			return base.Create();
		}
    }
}