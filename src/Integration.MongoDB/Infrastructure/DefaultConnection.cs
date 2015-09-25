using System;
using Castle.MicroKernel;
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

		protected internal override MongoUrl CreateMongoUrl(IKernel kernel)
		{
			if (_connection != null)
				return _connection.CreateMongoUrl(kernel);

			return base.CreateMongoUrl(kernel);
		}

		protected internal override IMongoClient Create(IKernel kernel)
		{
			if (_connection != null)
				return _connection.Create(kernel);

			return base.Create(kernel);
		}
    }
}