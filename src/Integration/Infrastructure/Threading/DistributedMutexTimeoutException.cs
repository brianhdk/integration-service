using System;
using System.Runtime.Serialization;

namespace Vertica.Integration.Infrastructure.Threading
{
    [Serializable]
    public class DistributedMutexTimeoutException : Exception
    {
        public DistributedMutexTimeoutException()
        {
        }

        public DistributedMutexTimeoutException(string message) : base(message)
        {
        }

        public DistributedMutexTimeoutException(string message, Exception inner) : base(message, inner)
        {
        }

        protected DistributedMutexTimeoutException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}