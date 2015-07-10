using MongoDB.Driver;

namespace Vertica.Integration.MongoDB.Infrastructure
{
    public interface IMongoDBClientFactory : IMongoDBClientFactory<DefaultConnection>
    {
    }

    public interface IMongoDBClientFactory<TConnection>
        where TConnection : Connection
    {
        IMongoClient Client { get; }
        IMongoDatabase Database { get; }
    }
}