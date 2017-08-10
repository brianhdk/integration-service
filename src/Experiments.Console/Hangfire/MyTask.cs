using System;
using Vertica.Integration.Model;

namespace Experiments.Console.Hangfire
{
    public class MyTask : Task
    {
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

            context.Log.Message("Done");
            context.Log.Message("...");
        }

        public override string Description => $@"{nameof(MyTask)}

New line

New line again";
    }
}