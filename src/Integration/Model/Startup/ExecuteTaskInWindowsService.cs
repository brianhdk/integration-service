using System;
using System.IO;
using System.ServiceProcess;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Windows;
using Vertica.Integration.Model.Web;

namespace Vertica.Integration.Model.Startup
{
    internal class ExecuteTaskInWindowsService : StartupAction
    {
        private readonly ITaskRunner _runner;
        private readonly ILogger _logger;

        public ExecuteTaskInWindowsService(ITaskRunner runner, ILogger logger)
        {
            if (runner == null) throw new ArgumentNullException("runner");
            if (logger == null) throw new ArgumentNullException("logger");

            _runner = runner;
            _logger = logger;
        }

        protected override string ActionName
        {
            get { return "service"; }
        }

        protected override ArgumentValidator Validator
        {
            get { return ArgumentValidator.WindowsService; }
        }

        protected override void DoExecute(ExecutionContext context)
        {
            Func<IDisposable> taskFactory;

            if (Argument.AbsoluteUrl.IsValid(context.ActionArguments[0]))
            {
                taskFactory = () => new WebApiHost(context.ActionArguments[0], TextWriter.Null, _logger, context.Task, context.TaskArguments);
            }
            else
            {
                uint seconds = UInt32.Parse(context.ActionArguments[0]);

                Action task = () => _runner.Execute(context.Task, context.TaskArguments);

                taskFactory = () => task.Repeat(Delay.Custom(seconds), TextWriter.Null);
            }

            using (var runner = new WindowsServiceRunner(context.TaskName, taskFactory))
            {
                ServiceBase.Run(runner);
            }
        }
    }
}