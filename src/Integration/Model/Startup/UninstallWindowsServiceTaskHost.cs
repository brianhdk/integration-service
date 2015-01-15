using Vertica.Integration.Infrastructure.Windows;

namespace Vertica.Integration.Model.Startup
{
    internal class UninstallWindowsServiceTaskHost : StartupAction
    {
        protected override string ActionName
        {
            get { return "uninstall"; }
        }

        protected override ArgumentValidator Validator
        {
            get { return null; }
        }

        protected override void DoExecute(ExecutionContext context)
        {
            using (var installer = new WindowsServiceInstaller(context.TaskName, context.Task.DisplayName))
            {
                installer.Uninstall();
            }
        }
    }
}