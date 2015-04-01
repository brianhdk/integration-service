using System;
using Castle.Windsor;
using Vertica.Integration.Model.Web;

namespace Vertica.Integration.Startup
{
    internal class StartWebApiHost : StartupAction
    {
        public StartWebApiHost(IWindsorContainer container)
            : base(container)
        {
        }

        public override bool IsSatisfiedBy(ExecutionContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            return context.Task is WebApiTask && base.IsSatisfiedBy(context);
        }

        protected override string ActionName
        {
            get { return "url"; }
        }

        protected override ArgumentValidator Validator
        {
            get { return new ArgumentValidator("[absolute-url]", Argument.AbsoluteUrl); }
        }

        protected override void DoExecute(ExecutionContext context)
        {
            using (new WebApiHost(context.ActionArguments[0], context.Task, Console.Out, Container))
            {
                do
                {
                    Console.WriteLine();
                    Console.WriteLine(@"Press ESCAPE to stop web-service..");
                    Console.WriteLine();

                } while (WaitingForEscape());
            }
        }

        private static bool WaitingForEscape()
        {
            return Console.ReadKey(intercept: true /* don't display */).Key != ConsoleKey.Escape;
        }
    }
}