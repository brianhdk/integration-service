using System;
using System.Collections;
using TaskScheduler;

namespace Vertica.Integration.Infrastructure.Windows
{
	internal class ScheduledTasks : IScheduledTasks
	{
		private readonly ITaskService _taskService;

		public ScheduledTasks(string machineName = null)
		{
			_taskService = new TaskScheduler.TaskScheduler();

			if (String.IsNullOrWhiteSpace(machineName))
			{
				_taskService.Connect();
			}
			else
			{
				_taskService.Connect(machineName);
			}
		}

		public void InstallOrUpdate(ScheduledTaskConfiguration scheduledTask)
		{
			if (scheduledTask == null) throw new ArgumentNullException("scheduledTask");

			ITaskDefinition taskDefinition = _taskService.NewTask(0);
			scheduledTask.Initialize(taskDefinition);
			InstallOrUpdate(taskDefinition, scheduledTask);
		}

		public void Uninstall(ScheduledTaskConfiguration scheduledTask)
		{
			if (scheduledTask == null) throw new ArgumentNullException("scheduledTask");

			Uninstall(scheduledTask.Name, scheduledTask.Folder);
		}

		public void Uninstall(string name, string folder)
		{
			if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");
			if (String.IsNullOrWhiteSpace(folder)) throw new ArgumentException(@"Value cannot be null or empty.", "folder");

			ITaskFolder actualFolder = _taskService.GetFolder(folder);
			actualFolder.DeleteTask(name, 0);
		}

		private void InstallOrUpdate(ITaskDefinition taskDefinition, ScheduledTaskConfiguration scheduledTask)
		{
			if (scheduledTask.Credentials == null)
				throw new InvalidOperationException("No Credentials defined.");

			ITaskFolder folder = GetOrCreateFolder(scheduledTask.Folder);

			folder.RegisterTaskDefinition(
				Path: scheduledTask.Name, 
				pDefinition: taskDefinition, 
				flags: Convert.ToInt32(_TASK_CREATION.TASK_CREATE_OR_UPDATE), 
				UserId: scheduledTask.Credentials.Username, 
				password: scheduledTask.Credentials.Password,
				LogonType: scheduledTask.GetLogonType());
		}

		private ITaskFolder GetOrCreateFolder(string folder)
		{
			if (folder == null) throw new ArgumentNullException("folder");

			ITaskFolder rootFolder = _taskService.GetFolder(@"\");
			IEnumerator subfolders = rootFolder.GetFolders(0).GetEnumerator();
			bool folderExists = false;
			ITaskFolder actualFolder = null;

			while (subfolders.MoveNext() && !folderExists)
			{
				actualFolder = ((ITaskFolder)subfolders.Current);
				folderExists = actualFolder.Name.Equals(folder, StringComparison.OrdinalIgnoreCase);
			}

			if (!folderExists)
				actualFolder = rootFolder.CreateFolder(folder);

			return actualFolder;
		}
	}
}
