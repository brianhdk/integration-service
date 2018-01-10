using System;
using System.Linq;
using Hangfire.Console;
using Hangfire.Console.Progress;
using Vertica.Integration.Hangfire.Console;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Tasks;

namespace Experiments.Console.Hangfire
{
    [PreventConcurrentTaskExecution]
    public class MyTaskWithProgressBar : IntegrationTask
    {
        private readonly IHangfirePerformContextProvider _hangfireContextProvider;

        public MyTaskWithProgressBar(IHangfirePerformContextProvider hangfireContextProvider)
        {
            _hangfireContextProvider = hangfireContextProvider;
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            uint count;
            if (!uint.TryParse(context.Arguments["Iterations"], out count))
                count = 10;

            int[] iterations = Enumerable.Range(1, (int) count).ToArray();

            IProgressBar progressBar = _hangfireContextProvider.Current.WriteProgressBar();

            foreach (int i in iterations.WithProgress(progressBar))
            {
                context.ThrowIfCancelled();

                // Everything we log will be outputted in the Hangfire UI/dashboard
                //  - This was enabled by the .EnableConsole() method on .UseHangfire()
                context.Log.Message("Iteration: [{0}/{1}]", i, iterations.Length);

                if (i != iterations.Length)
                    context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));
            }

            context.Log.Message("Done");
            context.Log.Message("...");
        }

        public override string Description => nameof(MyTaskWithProgressBar);
    }
}