using System;

namespace Vertica.Integration.Model
{
	public interface IStep
	{
		string Description { get; }
	}

	public interface IStep<in TWorkItem> : IStep
	{
	    [Obsolete("Use Execute(ITaskExcecutionContext context)")]
        Execution ContinueWith(TWorkItem workItem, ITaskExecutionContext context);

	    [Obsolete("Use Execute(ITaskExcecutionContext<TWorkItem> context)")]
	    void Execute(TWorkItem workItem, ITaskExecutionContext context);

	    Execution ContinueWith(ITaskExecutionContext<TWorkItem> context);

	    void Execute(ITaskExecutionContext<TWorkItem> context);
	}
}