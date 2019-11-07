using System;
using System.IO;
using System.Text;

namespace Vertica.Integration.Infrastructure.Remote.Ftp
{
    public static class FtpClientExtensions
    {
	    public static string DownloadToMemoryStream(this IFtpClient client, string name, MemoryStream memoryStream)
	    {
		    if (client == null) throw new ArgumentNullException(nameof(client));
		    if (memoryStream == null) throw new ArgumentNullException(nameof(memoryStream));

		    return client.Download(name, stream =>
		    {
			    stream.CopyTo(memoryStream);
			    memoryStream.Position = 0;
		    });
	    }

	    public static string DownloadToLocal(this IFtpClient client, string name, DirectoryInfo localDirectory)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (localDirectory == null) throw new ArgumentNullException(nameof(localDirectory));

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
            if (client == null) throw new ArgumentNullException(nameof(client));

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
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (localFile == null) throw new ArgumentNullException(nameof(localFile));

            using (FileStream stream = localFile.OpenRead())
            {
                return client.Upload(localFile.Name, stream);
            }
        }

        public static string UploadFromString(this IFtpClient client, string name, string content, Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(name));

            using (var stream = new MemoryStream())
            using (var writer = encoding == null ? new StreamWriter(stream) : new StreamWriter(stream, encoding))
            {
                writer.Write(content ?? string.Empty);
                writer.Flush();
                stream.Position = 0;

                return client.Upload(name, stream, binary: false);
            }
        }
    }
}