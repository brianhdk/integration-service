using System;
using System.IO;
using System.ServiceProcess;
using Castle.Windsor;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Windows;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Web;

namespace Vertica.Integration.Startup
{
    internal class RunTaskFromWindowsService : StartupAction
    {
        public RunTaskFromWindowsService(IWindsorContainer container) 
            : base(container)
        {
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
                taskFactory = () => new WebApiHost(context.ActionArguments[0], TextWriter.Null, Resolve<ILogger>(), context.Task, context.TaskArguments);
            }
            else
            {
                uint seconds = UInt32.Parse(context.ActionArguments[0]);

                Action task = () => Resolve<ITaskRunner>().Execute(context.Task, context.TaskArguments);

                taskFactory = () => task.Repeat(Delay.Custom(seconds), TextWriter.Null);
            }

            using (var runner = new WindowsServiceRunner(context.TaskName, taskFactory))
            {
                ServiceBase.Run(runner);
            }
        }
    }
}