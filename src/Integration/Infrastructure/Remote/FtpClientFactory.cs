using System;
using Vertica.Integration.Infrastructure.Remote.Ftp;

namespace Vertica.Integration.Infrastructure.Remote
{
    public class FtpClientFactory : IFtpClientFactory
    {
        public IFtpClient Create(Uri ftpUri, Action<FtpClientConfiguration> ftp = null)
        {
            var configuration = new FtpClientConfiguration(ftpUri);

            if (ftp != null)
                ftp(configuration);

            return new FtpClient(configuration);
        }

        public IFtpClient Create(string ftpUri, Action<FtpClientConfiguration> ftp = null)
        {
            var configuration = new FtpClientConfiguration(ftpUri);

            if (ftp != null)
                ftp(configuration);

            return new FtpClient(configuration);
        }
    }
}