using System;
using Microsoft.Web.Administration;

namespace Vertica.Integration.IIS
{
    public static class IISExtensions
    {
        public static ApplicationConfiguration UseIIS(this ApplicationConfiguration application, Action<IISConfiguration> iis = null)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

            return application.Extensibility(extensibility =>
            {
                IISConfiguration configuration = extensibility.Register(() => new IISConfiguration(application));

                iis?.Invoke(configuration);
            });
        }
    }

    public interface IServerManagerFactory
    {
        ServerManager Create();
    }

    public class ServerManagerFactory : IServerManagerFactory
    {
        public ServerManager Create()
        {
            return new ServerManager(); // local IIS

            //return ServerManager.OpenRemote("maersk-web01.vertica.dk");
        }
    }
}