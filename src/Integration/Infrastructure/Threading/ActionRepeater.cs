using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Vertica.Integration.Infrastructure.Threading
{
	public class ActionRepeater : IDisposable
    {
        private readonly TimeSpan _delay;
	    private readonly CancellationToken _cancellationToken;
	    private readonly TextWriter _outputter;

        private Task _task;
        private bool _repeating = true;

        private ActionRepeater(TimeSpan delay, CancellationToken cancellationToken, TextWriter outputter)
        {
            _delay = delay;
            _cancellationToken = cancellationToken;
            _outputter = outputter ?? TextWriter.Null;
        }

        private async void Repeat(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            int iterations = 0;

            _outputter.WriteLine("Starting Repeater (delay: {0})", _delay);

            _task = Task.Factory.StartNew(() =>
            {
                while (_repeating && !_cancellationToken.IsCancellationRequested)
                {
                    action();
                    iterations++;

                    _cancellationToken.WaitHandle.WaitOne(_delay);
                }

            }, _cancellationToken);

            await _task;

            _outputter.WriteLine("Repeater stopped after {0} iterations.", iterations);
        }

        public void Dispose()
        {
            _repeating = false;

            _outputter.WriteLine("Waiting for Repeater to stop.");

            try
            {
                _task.Wait(_cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }

            _task.Dispose();
            _task = null;
        }

        public static ActionRepeater Start(Action task, TimeSpan delay, CancellationToken cancellationToken, TextWriter outputter)
        {
            var repeater = new ActionRepeater(delay, cancellationToken, outputter);
            repeater.Repeat(task);

            return repeater;
        }
    }
}