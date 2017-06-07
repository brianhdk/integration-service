using Microsoft.Web.Administration;

namespace Vertica.Integration.IIS
{
    public interface IServerManagerFactory
    {
        ServerManager Create(string serverName = null);
    }
}