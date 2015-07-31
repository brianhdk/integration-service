using System;
using MongoDB.Driver;
using Vertica.Integration.MongoDB.Infrastructure;

namespace Vertica.Integration.MongoDB
{
    internal class MongoDBClientFactory<TConnection> : IMongoDBClientFactory<TConnection>
        where TConnection : Connection
    {
        private readonly Lazy<IMongoClient> _client;
        private readonly Lazy<IMongoDatabase> _database;

        public MongoDBClientFactory(Connection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            _client = new Lazy<IMongoClient>(() =>
            {
                IMongoClient client = connection.Create();
                client.Settings.Freeze();

                return client;
            });

            _database = new Lazy<IMongoDatabase>(() =>
            {
                if (String.IsNullOrWhiteSpace(connection.MongoUrl.DatabaseName))
                    return null;

                return Client.GetDatabase(connection.MongoUrl.DatabaseName);
            });
        }

        public IMongoClient Client
        {
            get { return _client.Value; }
        }

        public IMongoDatabase Database
        {
            get { return _database.Value; }
        }
    }

    internal class MongoDBClientFactory : IMongoDBClientFactory
    {
        private readonly IMongoDBClientFactory<DefaultConnection> _decoree;

        public MongoDBClientFactory(IMongoDBClientFactory<DefaultConnection> decoree)
        {
            if (decoree == null) throw new ArgumentNullException("decoree");

            _decoree = decoree;
        }

        public IMongoClient Client
        {
            get { return _decoree.Client; }
        }

        public IMongoDatabase Database
        {
            get { return _decoree.Database; }
        }
    }
}