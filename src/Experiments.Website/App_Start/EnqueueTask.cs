using System;
using System.Collections.Generic;
using Vertica.Integration.Model;

namespace Experiments.Website
{
    public class EnqueueTask : IEnqueueTask
    {
        private readonly ITaskFactory _factory;
        private readonly ITaskRunner _runner;

        public EnqueueTask(ITaskFactory factory, ITaskRunner runner)
        {
            _factory = factory;
            _runner = runner;
        }

        public void Run(string taskName, params KeyValuePair<string, string>[] arguments)
        {
            if (string.IsNullOrWhiteSpace(taskName)) throw new ArgumentException(@"Value cannot be null or empty", nameof(taskName));

            _runner.Execute(_factory.Get(taskName), new Arguments(arguments));
        }
    }
}