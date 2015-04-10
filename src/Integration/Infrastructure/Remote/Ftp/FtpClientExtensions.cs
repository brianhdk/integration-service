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

        public static string DownloadToString(this IFtpClient client, string name)
        {
            if (client == null) throw new ArgumentNullException("client");

            string content = null;

            client.Download(name, stream =>
            {
                using (var reader = new StreamReader(stream))
                {
                    content = reader.ReadToEnd();
                }
            });

            return content;
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

        public static string UploadFromString(this IFtpClient client, string name, string content)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(content ?? String.Empty);
                writer.Flush();
                stream.Position = 0;

                return client.Upload(name, stream, binary: false);
            }
        }
    }
}