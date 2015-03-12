using System;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Portal.Models
{
    public class ErrorLogModel
    {
        public int Id { get; set; }
        public Severity Severity { get; set; }
        public string Message { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public string Target { get; set; }
    }
}