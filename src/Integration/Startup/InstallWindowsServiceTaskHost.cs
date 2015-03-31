using System.Linq;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Windows;

namespace Vertica.Integration.Startup
{
    internal class InstallWindowsServiceTaskHost : StartupAction
    {
        public InstallWindowsServiceTaskHost(IWindsorContainer container)
            : base(container)
        {
        }

        protected override string ActionName
        {
            get { return "install"; }
        }

        protected override ArgumentValidator Validator
        {
            get { return ArgumentValidator.WindowsService; }
        }

        protected override void DoExecute(ExecutionContext context)
        {
            string[] serviceArguments = 
                new [] { context.TaskName }.Concat(
                    context.TaskArguments.Concat(
                        new[] { "-service" }).Concat(
                            context.ActionArguments))
                    .ToArray();

            string taskName = context.Task.Name();

            using (var installer = new WindowsServiceInstaller(taskName, taskName))
            {
                installer.Install(context.Task.Description, serviceArguments);
            }
        }
    }
}