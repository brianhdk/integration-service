using System;
using System.Runtime.Serialization;
using Vertica.Integration.Startup;

namespace Vertica.Integration.Model.Exceptions
{
    [Serializable]
    internal class StartupActionNotFoundException : Exception
    {
        internal StartupActionNotFoundException(ExecutionContext context)
            : base(
                String.Format("No startup actions found supporting the execution context. Task: {0}, TaskArguments: {1}, Action: {2}, ActionArguments: {3}.", 
                    context.TaskName,
                    String.Join(" ", context.TaskArguments),
                    context.ActionName,
                    String.Join(" ", context.ActionArguments)))
        {
        }

        public StartupActionNotFoundException()
        {
        }

        protected StartupActionNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}