using System;
using System.Runtime.Serialization;

namespace Vertica.Integration.Infrastructure.Database
{
    [Serializable]
    public class DatabaseDisabledException : Exception
    {
        public DatabaseDisabledException()
        {
        }

        public DatabaseDisabledException(string message) : base(message)
        {
        }

        public DatabaseDisabledException(string message, Exception inner) : base(message, inner)
        {
        }

        protected DatabaseDisabledException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}