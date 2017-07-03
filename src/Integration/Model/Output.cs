using System;

namespace Vertica.Integration.Model
{
    internal class Output
    {
        private readonly Action<string> _message;

        public Output(Action<string> message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            _message = message;
        }

        public void Message(string format, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                _message(string.Format(format, args));
            }
            else
            {
                _message(format);
            }
        }
    }
}