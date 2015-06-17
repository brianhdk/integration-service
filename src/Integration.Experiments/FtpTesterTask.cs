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

        public override void StartTask(ITaskExecutionContext context)
        {
            IFtpClient client = _factory.Create("ftp://ftp.vertica.dk/BHK", ftp => ftp
                .Credentials("GuestFTP", "VerticaPass1010"));

            foreach (string item in client.ListDirectory())
            {
                context.Log.Message(item);
            }
        }

        public override string Description
        {
            get { return "TBD"; }
        }
    }
}