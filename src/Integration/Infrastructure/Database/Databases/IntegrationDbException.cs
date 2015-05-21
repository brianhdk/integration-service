using System;
using System.Runtime.Serialization;

namespace Vertica.Integration.Infrastructure.Database.Databases
{
    [Serializable]
    public class IntegrationDbException : Exception
    {
        public IntegrationDbException()
        {
        }

        public IntegrationDbException(string message)
            : base(message)
        {
        }

        public IntegrationDbException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected IntegrationDbException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}