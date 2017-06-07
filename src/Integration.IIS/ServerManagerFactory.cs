using Microsoft.Web.Administration;

namespace Vertica.Integration.IIS
{
    public class ServerManagerFactory : IServerManagerFactory
    {
        public ServerManager Create(string serverName = null)
        {
            if (string.IsNullOrWhiteSpace(serverName))
                return new ServerManager(); // local IIS

            return ServerManager.OpenRemote(serverName);
        }
    }
}