using System;

namespace Vertica.Integration.Portal.Models
{
    public class ErrorLogDetailedModel
    {
        public int Id { get;  set; }
        public string MachineName { get; set; }
        public string IdentityName { get; set; }
        public string CommandLine { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
        public string FormattedMessage { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public string Target { get; set; }
    }
}