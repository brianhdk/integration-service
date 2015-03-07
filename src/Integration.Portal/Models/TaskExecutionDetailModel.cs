using System;

namespace Vertica.Integration.Portal.Models
{
    public class TaskExecutionDetailModel
    {
        public int Id { get; internal set; }
        public string TaskName { get; private set; }
        public double ExecutionTimeSeconds { get; protected set; }
        public DateTimeOffset TimeStamp { get; private set; }
        public virtual string Message { get; protected set; }
        public virtual string StepName { get; protected set; }
    }
}