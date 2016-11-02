using System;
using System.Runtime.Serialization;

namespace Vertica.Integration.Slack.Messaging
{
    [Serializable]
    public class MessageHandlerNotFoundException : Exception
    {
        public MessageHandlerNotFoundException()
        {
        }

        public MessageHandlerNotFoundException(string message)
            : base(message)
        {
        }

        public MessageHandlerNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        protected MessageHandlerNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}