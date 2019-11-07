using System.Net.Mail;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Domain.Monitoring
{
    public class MonitorTarget : Target
    {
        public MonitorTarget(string name)
            : base(name)
        {
            ReceiveErrorsWithMessagesContaining = new string[0];
        }

        public string[] Recipients { get; set; }
        public MailPriority? MailPriority { get; set; }
        public string[] ReceiveErrorsWithMessagesContaining { get; set; }
    }
}