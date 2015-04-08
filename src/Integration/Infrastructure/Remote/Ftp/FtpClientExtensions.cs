using System;
using System.IO;

namespace Vertica.Integration.Infrastructure.Remote.Ftp
{
    public static class FtpClientExtensions
    {
        public static string DownloadToLocal(this IFtpClient client, string name, DirectoryInfo localDirectory)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (localDirectory == null) throw new ArgumentNullException("localDirectory");

            return client.Download(name, stream =>
            {
                using (var fileStream = new FileStream(Path.Combine(localDirectory.FullName, name), FileMode.Create))
                {
                    stream.CopyTo(fileStream);
                }
            });
        }

        public static string UploadFromLocal(this IFtpClient client, FileInfo localFile)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (localFile == null) throw new ArgumentNullException("localFile");

            using (FileStream stream = localFile.OpenRead())
            {
                return client.Upload(localFile.Name, stream);
            }
        }
    }
}