using System;

namespace Vertica.Integration.Portal.Models
{
    public class TaskLogModel
    {
        public int Id { get; set; }
        public string TaskName { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public string Message { get; set; }
        public string StepName { get; set; }
    }
}