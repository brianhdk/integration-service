using System;
using System.Threading;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Tasks;

namespace Experiments.Console.Hangfire
{
    [PreventConcurrentTaskExecution]
    internal class MyTask : IntegrationTask
    {
        private int _iterations;

        public override void StartTask(ITaskExecutionContext context)
        {
            int iterations = 10;

            for (int i = 1; i <= iterations; i++)
            {
                context.ThrowIfCancelled();

                // Everything we log will be outputted in the Hangfire UI/dashboard
                //  - This was enabled by the .EnableConsole() method on .UseHangfire()
                context.Log.Message("Iteration: [{0}/{1}]", i, iterations);

                if (i != iterations)
                    context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));
            }

            if (Interlocked.Increment(ref _iterations) % 4 == 0)
                throw new InvalidOperationException("Simulating that the task is throwing an exception. {json}");

            context.Log.Message("Done - including some {json}");
            context.Log.Message("...");
        }

        public override string Description => $@"{nameof(MyTask)}

New line

New line again";
    }
}