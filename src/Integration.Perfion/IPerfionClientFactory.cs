using Vertica.Integration.Perfion.Infrastructure.Client;

namespace Vertica.Integration.Perfion
{
    public interface IPerfionClientFactory<TConnection>
        where TConnection : Connection
    {
        IPerfionClient Client { get; }
    }

    public interface IPerfionClientFactory : IPerfionClientFactory<DefaultConnection>
    {
    }
}