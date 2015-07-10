using Vertica.Integration.MongoDB.Infrastructure;

namespace Vertica.Integration.MongoDB
{
    public interface IAdditionalConfiguration
    {
        IAdditionalConfiguration AddConnection<TConnection>(TConnection connection)
            where TConnection : Connection;
    }
}