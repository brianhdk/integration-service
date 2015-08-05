using System;
using System.Collections.Generic;
using System.ServiceProcess;
using Vertica.Integration.Infrastructure.Extensions;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public class ScheduleTask
	{
		public string FolderName { get; set; }
		public Credentials Credentials { get; private set; }
		public string Name { get; private set; }
		public string Description { get; private set; }
		public List<Action> Actions { get; private set; }
		public List<Trigger> Triggers { get; private set; }

		private const string DefaultFolder = "Integration Service";

		public ScheduleTask(ITask task, string folderName = null, Credentials credentials = null)
			: this(task.Name(), task.Description, folderName, credentials)
		{

		}

		public ScheduleTask(string name, string description, string folderName = null, Credentials credentials = null)
		{
			if (name == null) throw new ArgumentNullException("name");

			Name = name;
			Description = description;
			FolderName = folderName ?? DefaultFolder;
			Credentials = credentials;
			Actions = new List<Action>();
			Triggers = new List<Trigger>();
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
}
