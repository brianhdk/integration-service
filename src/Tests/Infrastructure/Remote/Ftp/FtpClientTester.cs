using NUnit.Framework;
using Vertica.Integration.Infrastructure.Remote;
using Vertica.Integration.Infrastructure.Remote.Ftp;

namespace Vertica.Integration.Tests.Infrastructure.Remote.Ftp
{
    [TestFixture]
    public class FtpClientTester
    {
        [Test]
        public void Test()
        {
            IFtpClient client = new FtpClientFactory().Create("ftp://ftp.vertica.dk/GuestFTP", builder => builder
                .Credentials("GuestFTP", "VerticaPass1010"));

            client.ListDirectory();
        }
    }
}