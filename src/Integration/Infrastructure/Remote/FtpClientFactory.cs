using System;
using Vertica.Integration.Infrastructure.Remote.Ftp;

namespace Vertica.Integration.Infrastructure.Remote
{
    public class FtpClientFactory : IFtpClientFactory
    {
        public IFtpClient Create(Uri ftpUri, Action<FtpClientConfiguration> builder = null)
        {
            var configuration = new FtpClientConfiguration(ftpUri);

            if (builder != null)
                builder(configuration);

            return new FtpClient(configuration);
        }

        public IFtpClient Create(string ftpUri, Action<FtpClientConfiguration> builder = null)
        {
            var configuration = new FtpClientConfiguration(ftpUri);

            if (builder != null)
                builder(configuration);

            return new FtpClient(configuration);
        }
    }
}