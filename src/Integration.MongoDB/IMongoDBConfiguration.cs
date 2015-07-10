using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.MongoDB
{
    public interface IMongoDBConfiguration
    {
        IAdditionalConfiguration Connection(ConnectionString connectionString);
    }
}