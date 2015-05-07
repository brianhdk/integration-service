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
		TWorkItem Start(ILog log, params string[] arguments);
		void End(TWorkItem workItem, ILog log, params string[] arguments);

		new IEnumerable<IStep<TWorkItem>> Steps { get; }
	}
}