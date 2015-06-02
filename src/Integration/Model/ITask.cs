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
        void End(TWorkItem workItem, ITaskExecutionContext context);

        new IEnumerable<IStep<TWorkItem>> Steps { get; }
    }
}