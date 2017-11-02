using System;
using System.Collections.Generic;

namespace Vertica.Integration.Model
{
	public interface ITask
	{
	    string Description { get; }

        IEnumerable<IStep> Steps { get; }
	}

    public interface ITask<TWorkItem> : ITask
    {
        TWorkItem Start(ITaskExecutionContext context);

        [Obsolete("Use End(ITaskExcecutionContext<TWorkItem> context) instead.")]
        void End(TWorkItem workItem, ITaskExecutionContext context);

        void End(ITaskExecutionContext<TWorkItem> context);

        new IEnumerable<IStep<TWorkItem>> Steps { get; }
    }
}