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
            : this(
	            $"Method '{method}' failed. Path: '{currentPath}'. Message: {inner.Message}. See inner exception for more details.", inner)
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