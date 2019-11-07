using MongoDB.Driver;
using Vertica.Integration.MongoDB.Infrastructure;

namespace Vertica.Integration.MongoDB
{
    public interface IMongoDbClientFactory : IMongoDbClientFactory<DefaultConnection>
    {
    }

    public interface IMongoDbClientFactory<TConnection>
        where TConnection : Connection
    {
        IMongoClient Client { get; }
        IMongoDatabase Database { get; }
    }
}