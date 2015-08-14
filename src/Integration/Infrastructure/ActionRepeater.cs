using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Vertica.Integration.Infrastructure
{
	internal class ActionRepeater : IDisposable
    {
        private readonly TimeSpan _delay;
        private readonly TextWriter _outputter;

        private Task _task;
        private bool _repeating = true;

        private ActionRepeater(TimeSpan delay, TextWriter outputter)
        {
            _delay = delay;
            _outputter = outputter ?? TextWriter.Null;
        }

        private async void Repeat(Action action)
        {
            if (action == null) throw new ArgumentNullException("action");

            int iterations = 0;

            _outputter.WriteLine("Starting Repeater (delay: {0})", _delay);

            _task = Task.Factory.StartNew(() =>
            {
                while (_repeating)
                {
                    action();
                    iterations++;

                    Thread.Sleep(_delay);
                }

            }, TaskCreationOptions.LongRunning);

            await _task;

            _outputter.WriteLine("Repeater stopped after {0} iterations.", iterations);
        }

        public void Dispose()
        {
            _repeating = false;

            _outputter.WriteLine("Waiting for Repeater to stop.");
            Task.WaitAll(_task);

            _task.Dispose();
            _task = null;
        }

        public static ActionRepeater Start(Action task, TimeSpan delay, TextWriter outputter)
        {
            var repeater = new ActionRepeater(delay, outputter);

            repeater.Repeat(task);

            return repeater;
        }
    }
}