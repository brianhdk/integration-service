using System;
using System.Runtime.Serialization;

namespace Vertica.Integration.Model.Exceptions
{
    [Serializable]
    public class TaskExecutionFailedException : Exception
    {
        public TaskExecutionFailedException()
        {
        }

        public TaskExecutionFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected TaskExecutionFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}