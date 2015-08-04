using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using Vertica.Integration.Infrastructure.Extensions;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public class ScheduleTask
	{
		public string FolderName { get; private set; }
		public Credentials Credentials { get; private set; }
		public string Name { get; private set; }
		public string Description { get; private set; }
		public List<Action> Actions { get; private set; }

		private const string DefaultFolder = "Integration Service";

		public ScheduleTask(ITask task, string folderName = null, Credentials credentials = null)
		{
			if (task == null) throw new ArgumentNullException("task");

			Name = task.Name();
			Description = task.Description;
			FolderName = folderName ?? DefaultFolder;
			Credentials = credentials;
			Actions = new List<Action>();
		}

		public ScheduleTask WithCredentials(string username, string password)
		{
			if (String.IsNullOrWhiteSpace(username)) throw new ArgumentException(@"Value cannot be null or empty.", "username");
			if (String.IsNullOrWhiteSpace(password)) throw new ArgumentException(@"Value cannot be null or empty.", "password");

			Credentials = new Credentials
			{
				Account = ServiceAccount.User,
				Username = username,
				Password = password
			};

			return this;
		}
	}

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

	public class ExecuteTaskAction : Action
	{
		public ExecuteTaskAction(ITask task, Arguments arguments)
			: base(
			exePath: Assembly.GetEntryAssembly().Location, 
			arguments: String.Format("{0} {1}", task.Name(), arguments))
		{
		}
	}
}
