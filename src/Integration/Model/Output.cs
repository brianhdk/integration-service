using System;

namespace Vertica.Integration.Model
{
    public class Output
    {
        private readonly Action<string> _outputMessage;

        public Output(Action<string> outputMessage)
        {
            _outputMessage = outputMessage;
        }

        public void Message(string format, params object[] args)
        {
            _outputMessage(String.Format(format, args));
        }
    }
}