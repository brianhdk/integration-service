using System;
using System.Runtime.Serialization;

namespace Vertica.Integration.Infrastructure.Remote.Ftp
{
    [Serializable]
    public class FtpClientException : Exception
    {
        public FtpClientException()
        {
        }

        public FtpClientException(string method, string currentPath, Exception inner)
            : this(String.Format("Method '{0}' failed. Path: '{1}'. Message: {2}. See inner exception for more details.", method, currentPath, inner.Message), inner)
        {
        }

        public FtpClientException(string message, Exception inner) : base(message, inner)
        {
        }

        protected FtpClientException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}