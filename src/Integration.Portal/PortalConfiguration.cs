using System;
using System.IO;

namespace Vertica.Integration.Portal
{
    public class PortalConfiguration
    {
        internal const string FolderName = "Portal.Html5";
        
        internal static readonly string BaseFolder = FindBaseFolder();

        private static string FindBaseFolder()
        {
            string binFolder = new FileInfo(typeof(PortalConfiguration).Assembly.Location).DirectoryName ?? String.Empty;

            binFolder = Path.Combine(binFolder, FolderName);

#if DEBUG
            string developmentFolder = Path.Combine(binFolder, @"..\..\..\..\Integration.Portal");

            if (Directory.Exists(developmentFolder))
                binFolder = developmentFolder;
#endif 

            return binFolder;
        }
    }
}