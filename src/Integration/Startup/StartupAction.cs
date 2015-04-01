using System;
using System.Linq;
using Castle.Windsor;

namespace Vertica.Integration.Startup
{
    internal abstract class StartupAction
    {
        private readonly IWindsorContainer _container;

        protected StartupAction(IWindsorContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");

            _container = container;
        }

        protected IWindsorContainer Container
        {
            get { return _container; }
        }

        protected T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        protected abstract string ActionName { get; }

        public virtual bool IsSatisfiedBy(ExecutionContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            return String.Equals(ActionName, context.ActionName, StringComparison.OrdinalIgnoreCase);
        }

        public void Execute(ExecutionContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            if (Validator != null)
            {
                if (Validator.Count != context.ActionArguments.Length)
                    throw new ArgumentException(
                        String.Format(@"Invalid number of arguments passed. Usage: -{0} {1}
Example: {2} -{0} {3}", ActionName, Validator.Usage, context.TaskName, String.Join(" ", Validator.Select(x => x.Example))));

                for (int i = 0; i < context.ActionArguments.Length; i++)
                {
                    string error;
                    if (!Validator[i].IsValid(context.ActionArguments[i], out error))
                        throw new ArgumentException(String.Format("{0} Usage: -{1} {2}", error, ActionName, Validator.Usage));
                }
            }

            DoExecute(context);
        }

        protected abstract ArgumentValidator Validator { get; }

        protected abstract void DoExecute(ExecutionContext context);
    }
}