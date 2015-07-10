using System;
using System.IO;
using System.IO.Compression;
using Vertica.Integration.Model.Web;

namespace Vertica.Integration.Portal
{
    public static class PortalExtensions
    {
        public static ApplicationConfiguration UsePortal(this ApplicationConfiguration builder, Action<PortalConfiguration> portal = null)
        {
            if (builder == null) throw new ArgumentNullException("builder");

            var configuration = new PortalConfiguration();

            if (portal != null)
                portal(configuration);

            if (!Directory.Exists(PortalConfiguration.Folder))
            {
                var zipFile = new FileInfo(PortalConfiguration.ZipFile);

                if (!zipFile.Exists)
                {
                    throw new InvalidOperationException(String.Format(
@"Expected the following zip-file '{0}' to be present in the following folder '{1}'. 
This zip is automatically added when installing the Vertica.Integration.Portal NuGet package. 
Try re-installing the package and/or make sure that the zip-file is included part of your deployment of this platform and placed in the root-folder.", zipFile.Name, zipFile.DirectoryName));
                }

                ZipFile.ExtractToDirectory(zipFile.FullName, PortalConfiguration.Folder);
            }

            builder.WebApi(x => x
                .Remove<HomeController>()
                .AddFromAssemblyOfThis<PortalConfiguration>());

            return builder;
        }
    }
}