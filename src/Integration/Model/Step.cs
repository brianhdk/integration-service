using System;

namespace Vertica.Integration.Model
{
    public abstract class Step<TWorkItem> : IStep<TWorkItem>
    {
        [Obsolete("Implement Execute(ITaskExcecutionContext<TWorkItem> context) instead.")]
        public virtual void Execute(TWorkItem workItem, ITaskExecutionContext context)
        {
        }

        [Obsolete("Implement ContinueWith(ITaskExcecutionContext<TWorkItem> context) instead.")]
        public virtual Execution ContinueWith(TWorkItem workItem, ITaskExecutionContext context)
        {
            if (workItem == null) throw new ArgumentNullException(nameof(workItem));
            if (context == null) throw new ArgumentNullException(nameof(context));

            return Execution.Execute;
        }

        public virtual Execution ContinueWith(ITaskExecutionContext<TWorkItem> context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

#pragma warning disable 618
            return ContinueWith(context.WorkItem, context);
#pragma warning restore 618
        }

        // TODO: When upgrading to next major version - make sure to change virtual to abstract - thus requiring clients to implement the Execute()-method.
        public virtual void Execute(ITaskExecutionContext<TWorkItem> context)
        {
#pragma warning disable 618
            Execute(context.WorkItem, context);
#pragma warning restore 618
        }

        public abstract string Description { get; }
    }
}