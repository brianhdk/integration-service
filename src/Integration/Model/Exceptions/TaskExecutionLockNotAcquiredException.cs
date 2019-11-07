using System;
using System.Runtime.Serialization;

namespace Vertica.Integration.Model.Exceptions
{
    [Serializable]
    public class TaskExecutionLockNotAcquiredException : Exception
    {
        public TaskExecutionLockNotAcquiredException()
        {
        }

        public TaskExecutionLockNotAcquiredException(string message) : base(message)
        {
        }

        public TaskExecutionLockNotAcquiredException(string message, Exception inner) : base(message, inner)
        {
        }

        protected TaskExecutionLockNotAcquiredException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}