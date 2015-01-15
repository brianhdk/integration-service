using System;
using System.Runtime.Serialization;

namespace Vertica.Integration.Model.Exceptions
{
    [Serializable]
    public class StartupActionNotFoundException : Exception
    {
        public StartupActionNotFoundException()
        {
        }

        public StartupActionNotFoundException(string[] arguments)
            : base(String.Format("No startup actions found supporting the execution context. {0}", String.Join(" ", arguments ?? new string[0])))
        {
        }

        protected StartupActionNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}