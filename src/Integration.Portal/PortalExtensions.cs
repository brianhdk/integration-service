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

            string assetsPath = Path.Combine(PortalConfiguration.BinFolder, PortalConfiguration.AssetsFolderName);

            if (!Directory.Exists(assetsPath))
            {
                string zipFile = Path.Combine(PortalConfiguration.BinFolder, PortalConfiguration.ZipFileName);

                if (!File.Exists(zipFile))
                {
                    throw new InvalidOperationException(String.Format(
@"Expected the following zip-file '{0}' to be present in the following folder '{1}'. 
This zip is automatically added when installing the Vertica.Integration.Portal NuGet package. 
Try re-installing the package.", PortalConfiguration.ZipFileName, PortalConfiguration.BinFolder));
                }

                ZipFile.ExtractToDirectory(zipFile, PortalConfiguration.BinFolder);
            }

            builder.WebApi(x => x
                .Remove<HomeController>()
                .Scan(typeof (PortalExtensions).Assembly));

            return builder;
        }
    }
}