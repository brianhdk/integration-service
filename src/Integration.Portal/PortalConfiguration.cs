using System;
using System.IO;
using System.Reflection;

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
            return new FileInfo(typeof(PortalConfiguration).Assembly.Location).DirectoryName ?? String.Empty;
        }

        private static string GetVersion()
        {
            AssemblyName name = typeof(PortalConfiguration).Assembly.GetName();

            return name.Version.ToString();
        }

        private static string FindFolder()
        {
            string folder = Path.Combine(BinFolder, String.Format("{0}-{1}", Name, Version));

#if DEBUG
            string developmentFolder = Path.Combine(folder, @"..\..\..\..\Integration.Portal");

            if (Directory.Exists(developmentFolder))
                folder = developmentFolder;
#endif 

            return folder;
        }

        private static string GetZipFile()
        {
            return Path.Combine(BinFolder, String.Format("{0}.zip", Name));
        }
    }
}