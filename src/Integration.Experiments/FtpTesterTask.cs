using Vertica.Integration.Infrastructure.Remote;
using Vertica.Integration.Infrastructure.Remote.Ftp;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class FtpTesterTask : Task
    {
        private readonly IFtpClientFactory _factory;

        public FtpTesterTask(IFtpClientFactory factory)
        {
            _factory = factory;
        }

        public override void StartTask(Log log, params string[] arguments)
        {
            IFtpClient client = _factory.Create("ftp://ftp.vertica.dk/BHK", builder => builder
                .Credentials("GuestFTP", "VerticaPass1010"));

            foreach (var item in client.ListDirectory())
            {
                log.Message(item);
            }
        }

        public override string Description
        {
            get { return "TBD"; }
        }
    }
}