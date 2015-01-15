using System;

namespace Vertica.Integration.Model
{
    public class Output
    {
        private readonly Action<string> _message;

        public Output(Action<string> message)
        {
            if (message == null) throw new ArgumentNullException("message");

            _message = message;
        }

        public void Message(string format, params object[] args)
        {
            _message(String.Format(format, args));
        }
    }
}