using System;

namespace Vertica.Integration.Model
{
    public abstract class Step<TWorkItem> : IStep<TWorkItem>
    {
        public abstract string Description { get; }

        public virtual Execution ContinueWith(TWorkItem workItem, ITaskExecutionContext context)
        {
            if (workItem == null) throw new ArgumentNullException(nameof(workItem));
            if (context == null) throw new ArgumentNullException(nameof(context));

            return Execution.Execute;
        }

        public abstract void Execute(TWorkItem workItem, ITaskExecutionContext context);
    }
}