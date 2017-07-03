using System;
using System.IO;
using System.Threading;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;

namespace Experiments.Console
{
    public class MyTask : Task
    {
        public override void StartTask(ITaskExecutionContext context)
        {
            FileInfo file = EnsureFile(context);

            if (file == null)
                return;

            if (file.Name.IndexOf("fail", StringComparison.OrdinalIgnoreCase) >= 0)
                throw new InvalidOperationException($"failed {file.Name}");

            context.Log.Message($"Name: {file.Name}, Size: {file.Length}");

            Thread.Sleep(300);

            file.Delete();
        }

        public FileInfo EnsureFile(ITaskExecutionContext context)
        {
            var file = new FileInfo(context.Arguments["File"]);

            int attempts = 10;

            while (--attempts > 0)
            {
                if (!file.Exists)
                    return null;

                if (!file.IsLocked())
                    return file;

                context.Log.Message("Waiting for file {0} to be unlocked [{1}].", file.Name, attempts);
                context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(1));
            }

            context.Log.Message("Unable to read file {0}", file.Name);
            return null;
        }

        public override string Description => nameof(MyTask);
    }
}