using System;
using Vertica.Integration.Infrastructure.Remote.Ftp;

namespace Vertica.Integration.Infrastructure.Remote
{
    public interface IFtpClientFactory
    {
        IFtpClient Create(Uri ftpUri, Action<FtpClientConfiguration> builder = null);
        IFtpClient Create(string ftpUri, Action<FtpClientConfiguration> builder = null);
    }
}