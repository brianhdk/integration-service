using System.IO;
using System.Reflection;
using System.Web;

namespace Vertica.Integration.Portal
{
    public class PortalConfiguration
    {
        private const string Name = "Portal.Html5";

        private static readonly string BinFolder = GetBinFolder();
        
        internal static readonly string Version = GetVersion();
        internal static readonly string Folder = FindFolder();
        internal static readonly string ZipFile = GetZipFile();

        private static string GetBinFolder()
        {
            if (IsWebApp())
                return HttpRuntime.AppDomainAppPath;

            return new FileInfo(typeof(PortalConfiguration).Assembly.Location).DirectoryName ?? string.Empty;
        }

        private static bool IsWebApp()
        {
            return !string.IsNullOrWhiteSpace(HttpRuntime.AppDomainAppId);
        }

        private static string GetVersion()
        {
            AssemblyName name = typeof(PortalConfiguration).Assembly.GetName();

            return name.Version.ToString();
        }

        private static string FindFolder()
        {
            string folder = Path.Combine(BinFolder, $"{Name}-{Version}");

#if DEBUG
            string developmentFolder = @"..\..\Integration.Portal";

            if (!IsWebApp())
                developmentFolder = $@"..\..\{developmentFolder}";

            developmentFolder = Path.Combine(folder, developmentFolder);

            if (Directory.Exists(developmentFolder))
                folder = developmentFolder;
#endif 

            return folder;
        }

        private static string GetZipFile()
        {
            return Path.Combine(BinFolder, $"{Name}.zip");
        }
    }
}