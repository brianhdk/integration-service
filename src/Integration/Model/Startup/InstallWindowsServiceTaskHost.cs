using System.Linq;
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

            using (var installer = new WindowsServiceInstaller(context.TaskName, context.Task.DisplayName))
            {
                installer.Install(context.Task.Description, serviceArguments);
            }
        }
    }
}