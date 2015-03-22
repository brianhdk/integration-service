using System.Linq;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Windows;

namespace Vertica.Integration.Model.Startup
{
    internal class InstallWindowsServiceTaskHost : StartupAction
    {
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