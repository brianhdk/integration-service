using System;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting.Handlers;
using Action = Vertica.Integration.Model.Hosting.Handlers.Action;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	public class ScheduleTaskConfiguration
	{
		private readonly ScheduleTask _scheduleTask;
		private readonly ScheduleTaskInstaller _installer;

		public ScheduleTaskConfiguration(ITask task, Credentials credentials, string folder = null)
		{
			if (task == null) throw new ArgumentNullException("task");

			_scheduleTask = new ScheduleTask(task, folder, credentials);
			_installer = new ScheduleTaskInstaller();
		}

		public ScheduleTaskConfiguration(string name, string description, Credentials credentials, string folder = null)
		{
			if (name == null) throw new ArgumentNullException("name");

			_scheduleTask = new ScheduleTask(name, description, folder, credentials);
			_installer = new ScheduleTaskInstaller();
		}

		public ScheduleTaskConfiguration AddTrigger(Trigger trigger)
		{
			if (trigger == null) throw new ArgumentNullException("trigger");

			_scheduleTask.Triggers.Add(trigger);
			return this;
		}

		public ScheduleTaskConfiguration AddAction(Action action)
		{
			if (action == null) throw new ArgumentNullException("action");

			_scheduleTask.Actions.Add(action);
			return this;
		}

		public ScheduleTaskConfiguration AddTaskAction<T>(Arguments arguments = null) where T : ITask
		{
			return AddAction(new ExecuteTaskAction<T>(arguments));
		}

		public void InstallOrUpdate()
		{
			_installer.InstallOrUpdate(_scheduleTask);
		}
	}
}
