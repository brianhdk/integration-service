using System;
using System.Collections.Generic;
using System.ServiceProcess;
using TaskScheduler;
using Vertica.Utilities_v4.Extensions.StringExt;

namespace Vertica.Integration.Infrastructure.Windows
{
	public class ScheduledTaskConfiguration
	{
		private readonly List<ScheduledTaskAction> _actions;
		private readonly List<ScheduledTaskTrigger> _triggers;

		private _TASK_LOGON_TYPE _logonType;
		private string _description;

		public ScheduledTaskConfiguration(string name, string folder, string exePath, string args)
		{
			if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");
			if (String.IsNullOrWhiteSpace(folder)) throw new ArgumentException(@"Value cannot be null or empty.", "folder");

			Name = name;
			Folder = folder;

			WithAccount(ServiceAccount.LocalSystem);

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
			if (trigger == null) throw new ArgumentNullException("trigger");

			_triggers.Add(trigger);

			return this;
		}

		public string Name { get; private set; }
		public string Folder { get; private set; }

		internal Credentials Credentials { get; private set; }

		public ScheduledTaskConfiguration Description(string description)
		{
			_description = description.NullIfEmpty();
			return this;
		}

		public ScheduledTaskConfiguration WithCredentials(string username, string password)
		{
			if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException(@"Value cannot be null or empty.", "username");
			if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException(@"Value cannot be null or empty.", "password");

			Credentials = new Credentials
			{
				Account = ServiceAccount.User,
				Username = username,
				Password = password
			};

			_logonType = _TASK_LOGON_TYPE.TASK_LOGON_PASSWORD;

			return this;
		}

		public ScheduledTaskConfiguration WithAccount(ServiceAccount account)
		{
			Credentials = new Credentials
			{
				Account = account
			};

			_logonType = _TASK_LOGON_TYPE.TASK_LOGON_SERVICE_ACCOUNT;

			return this;
		}
		
		internal void Initialize(ITaskDefinition task)
		{
			task.Settings.MultipleInstances = _TASK_INSTANCES_POLICY.TASK_INSTANCES_IGNORE_NEW;
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
			return _logonType;
		}
	}
}