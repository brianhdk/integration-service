using System;
using MongoDB.Driver;

namespace Vertica.Integration.MongoDB.Infrastructure
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

		protected internal override MongoUrl MongoUrl
		{
			get { return _connection.MongoUrl; }
		}

		protected internal override IMongoClient Create()
		{
			return _connection.Create();
		}
    }
}