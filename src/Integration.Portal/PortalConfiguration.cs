using System.IO;

namespace Vertica.Integration.Portal
{
    public class PortalConfiguration
    {
        internal static readonly string BinFolder = new FileInfo(typeof(PortalConfiguration).Assembly.Location).DirectoryName;
        internal const string AssetsFolderName = "Assets";
        internal const string ZipFileName = "Portal.Assets.zip";
    }
}