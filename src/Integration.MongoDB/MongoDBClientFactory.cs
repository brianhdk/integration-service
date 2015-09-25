using System;
using Castle.MicroKernel;
using MongoDB.Driver;
using Vertica.Integration.MongoDB.Infrastructure;

namespace Vertica.Integration.MongoDB
{
    internal class MongoDBClientFactory<TConnection> : IMongoDbClientFactory<TConnection>
        where TConnection : Connection
    {
        private readonly Lazy<IMongoClient> _client;
        private readonly Lazy<IMongoDatabase> _database;

		public MongoDBClientFactory(TConnection connection, IKernel kernel)
        {
            if (connection == null) throw new ArgumentNullException("connection");
			if (kernel == null) throw new ArgumentNullException("kernel");

			_client = new Lazy<IMongoClient>(() =>
            {
                IMongoClient client = connection.Create(kernel);
                client.Settings.Freeze();

                return client;
            });

            _database = new Lazy<IMongoDatabase>(() =>
            {
	            MongoUrl mongoUrl = connection.CreateMongoUrl(kernel);

	            if (mongoUrl == null)
		            return null;

                if (String.IsNullOrWhiteSpace(mongoUrl.DatabaseName))
                    return null;

                return Client.GetDatabase(mongoUrl.DatabaseName);
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

    internal class MongoDbClientFactory : IMongoDbClientFactory
    {
        private readonly IMongoDbClientFactory<DefaultConnection> _decoree;

        public MongoDbClientFactory(IMongoDbClientFactory<DefaultConnection> decoree)
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