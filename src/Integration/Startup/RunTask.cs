using System;
using Castle.Windsor;
using Vertica.Integration.Model;

namespace Vertica.Integration.Startup
{
    internal class RunTask : StartupAction
    {
        public RunTask(IWindsorContainer container)
            : base(container)
        {
        }

        protected override string ActionName
        {
            get { return String.Empty; }
        }

        protected override ArgumentValidator Validator
        {
            get { return null; }
        }

        protected override void DoExecute(ExecutionContext context)
        {
            Resolve<ITaskRunner>().Execute(context.Task, context.TaskArguments);
        }
    }
}