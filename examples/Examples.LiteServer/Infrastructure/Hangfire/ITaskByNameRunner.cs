using System.Collections.Generic;
using Hangfire;

namespace Examples.LiteServer.Infrastructure.Hangfire
{
    public interface ITaskByNameRunner
    {
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        void Run(string taskName, params KeyValuePair<string, string>[] arguments);
    }
}