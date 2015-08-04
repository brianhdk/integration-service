using System;
using System.Reflection;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public class Action
	{
		public Action(string exePath, string arguments)
		{
			Arguments = arguments;
			ExePath = exePath;
		}

		public string ExePath { get; private set; }
		public string Arguments { get; private set; }
	}

	public class ExecuteTaskAction<T> : Action where T: ITask
	{
		public ExecuteTaskAction(Arguments arguments = null)
			: base(
			exePath: Assembly.GetEntryAssembly().Location,
			arguments: String.Format("{0} {1}", typeof(T).Name, arguments ?? new Arguments()))
		{

		}
	}
}