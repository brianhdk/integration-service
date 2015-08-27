using System;
using Microsoft.Web.Administration;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.IIS
{
    public static class IISExtensions
    {
        public static ApplicationConfiguration UseIIS(this ApplicationConfiguration builder)
        {
            if (builder == null) throw new ArgumentNullException("builder");

			// undone - should introduce an IISConfiguration object and use same strategy as other extensions.
            builder.AddCustomInstallers(Install.ByConvention.AddFromAssemblyOfThis<IServerManagerFactory>());

            return builder;
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
            //return new ServerManager(); // local IIS

            return ServerManager.OpenRemote("maersk-web01.vertica.dk");
        }
    }
}