using System;
using System.Collections.Generic;
using System.ServiceProcess;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public class ScheduleTask
	{
		public string FolderName { get; private set; }
		public Credentials Credentials { get; private set; }
		public string Name { get; private set; }
		public string Description { get; private set; }
		public List<Action> Actions { get; private set; }

		public ScheduleTask(string name, string description, string folderName, Credentials credentials = null)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (folderName == null) throw new ArgumentNullException("folderName");

			Name = name;
			Description = description;
			FolderName = folderName;
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
		public string ExePath { get; private set; }
		public string Arguments { get; private set; }

		public Action(string exePath, string arguments = null)
		{
			if (exePath == null) throw new ArgumentNullException("exePath");

			ExePath = exePath;
			Arguments = arguments;
		}
	}
}
