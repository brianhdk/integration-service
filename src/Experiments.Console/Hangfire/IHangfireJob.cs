using System;
using System.Collections.Generic;
using Hangfire;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Model;

namespace Experiments.Console.Hangfire
{
    /// <summary>
    /// This interface typically exists in a shared library, shared by Integration Service and other "clients" - e.g. web-site project.
    /// This allows these above mentioned "clients" to create HF jobs that the Integration Service then can process.
    /// </summary>
    public interface IHangfireJob
    {
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        void RunTask(string taskName, params KeyValuePair<string, string>[] arguments);

        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        void RunMyTask();

        void WriteMessageToTheConsoleWriter(string format, params object[] args);
    }

    /// <summary>
    /// This class is implemented by the Integration Service project
    /// DI is enabled at this point, so you can depend on anything/any service.
    /// </summary>
    internal class HangfireJob : IHangfireJob
    {
        private readonly ITaskRunner _taskRunner;
        private readonly ITaskFactory _taskFactory;
        private readonly IConsoleWriter _consoleWriter;

        public HangfireJob(ITaskRunner taskRunner, ITaskFactory taskFactory, IConsoleWriter consoleWriter)
        {
            _taskRunner = taskRunner;
            _taskFactory = taskFactory;
            _consoleWriter = consoleWriter;
        }

        public void RunTask(string taskName, params KeyValuePair<string, string>[] arguments)
        {
            if (string.IsNullOrWhiteSpace(taskName)) throw new ArgumentException(@"Value cannot be null or empty", nameof(taskName));

            _taskRunner.Execute(_taskFactory.Get(taskName), new Arguments(arguments));
        }

        public void RunMyTask()
        {
            RunTask(nameof(MyTask));
        }

        public void WriteMessageToTheConsoleWriter(string format, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(format)) throw new ArgumentException(@"Value cannot be null or empty", nameof(format));

            _consoleWriter.WriteLine(format, args);
        }
    }
}