using System;
using System.Collections.Generic;
using System.ServiceProcess;
using TaskScheduler;
using Vertica.Integration.Infrastructure.Windows;
using Vertica.Utilities_v4.Extensions.StringExt;

namespace Vertica.Integration.WindowsTaskScheduler
{
	public class ScheduledTaskConfiguration
	{
		private readonly List<ScheduledTaskAction> _actions;
		private readonly List<ScheduledTaskTrigger> _triggers;

		private string _description;

		public ScheduledTaskConfiguration(string name, string folder, string exePath, string args)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(name));
			if (string.IsNullOrWhiteSpace(folder)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(folder));

			Name = name;
			Folder = folder;

			RunAs(ServiceAccount.LocalService);

			_actions = new List<ScheduledTaskAction>();
			_triggers = new List<ScheduledTaskTrigger>();

			AddAction(exePath, args);
		}

		public ScheduledTaskConfiguration AddAction(string exePath, string args)
		{
			_actions.Add(new ScheduledTaskAction(exePath, args));

			return this;
		}

		public ScheduledTaskConfiguration AddTrigger(ScheduledTaskTrigger trigger)
		{
			if (trigger == null) throw new ArgumentNullException(nameof(trigger));

			_triggers.Add(trigger);

			return this;
		}

		public string Name { get; private set; }
		public string Folder { get; private set; }

		public Credentials Credentials { get; private set; }

		public ScheduledTaskConfiguration Description(string description)
		{
			_description = description.NullIfEmpty();

			return this;
		}

		public ScheduledTaskConfiguration RunAsUser(string username, string password)
		{
			Credentials = new Credentials(username, password);

			return this;
		}

		public ScheduledTaskConfiguration RunAs(ServiceAccount account)
		{
			Credentials = new Credentials(account);

			return this;
		}
		
		internal void Initialize(ITaskDefinition task)
		{
			if (task == null) throw new ArgumentNullException(nameof(task));

			task.Settings.MultipleInstances = _TASK_INSTANCES_POLICY.TASK_INSTANCES_IGNORE_NEW;
			task.Settings.StopIfGoingOnBatteries = false;
			task.Settings.IdleSettings.StopOnIdleEnd = false;
			task.RegistrationInfo.Description = _description;
			task.Settings.Hidden = false;

			task.Actions.Clear();
			task.Triggers.Clear();

			foreach (ScheduledTaskAction action in _actions)
			{
				IExecAction taskAction = (IExecAction)task.Actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);

				taskAction.Path = action.ExePath;
				taskAction.Arguments = action.Args;
			}

			foreach (ScheduledTaskTrigger trigger in _triggers)
				trigger.AddToTask(task);
		}

		internal _TASK_LOGON_TYPE GetLogonType()
		{
			switch (Credentials.Account)
			{
				case ServiceAccount.LocalService:
				case ServiceAccount.NetworkService:
				case ServiceAccount.LocalSystem:
					return _TASK_LOGON_TYPE.TASK_LOGON_SERVICE_ACCOUNT;
				case ServiceAccount.User:
					return _TASK_LOGON_TYPE.TASK_LOGON_PASSWORD;
				default:
					return _TASK_LOGON_TYPE.TASK_LOGON_NONE;
			}
		}
	}
}