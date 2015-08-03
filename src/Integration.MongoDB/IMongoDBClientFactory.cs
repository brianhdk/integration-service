using MongoDB.Driver;
using Vertica.Integration.MongoDB.Infrastructure;

namespace Vertica.Integration.MongoDB
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