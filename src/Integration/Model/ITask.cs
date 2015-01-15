using System.Collections.Generic;

namespace Vertica.Integration.Model
{
	public interface ITask
	{
		string DisplayName { get; }
	    string Description { get; }
        string Schedule { get; }
		IEnumerable<IStep> Steps { get; }
	}

	public interface ITask<TWorkItem> : ITask
	{
		TWorkItem Start(Log log, params string[] arguments);
		void End(TWorkItem workItem, Log log, params string[] arguments);

		new IEnumerable<IStep<TWorkItem>> Steps { get; }
	}
}