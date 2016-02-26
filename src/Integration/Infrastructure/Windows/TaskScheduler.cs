using System;
using System.Collections;
using TaskScheduler;

namespace Vertica.Integration.Infrastructure.Windows
{
	internal class TaskScheduler : ITaskScheduler
	{
		private readonly ITaskService _taskService;

		public TaskScheduler(string machineName = null)
		{
			_taskService = new global::TaskScheduler.TaskScheduler();

			if (string.IsNullOrWhiteSpace(machineName))
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
			if (scheduledTask == null) throw new ArgumentNullException(nameof(scheduledTask));

			ITaskFolder folder = GetOrCreateFolder(scheduledTask.Folder);
			IRegisteredTask existingTask = folder.GetTask(scheduledTask.Name);
			ITaskDefinition task = existingTask != null ? existingTask.Definition : _taskService.NewTask(0);

			scheduledTask.Initialize(task);
			InstallOrUpdate(folder, task, scheduledTask);
		}

		public void Uninstall(string name, string folder)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(name));
			if (string.IsNullOrWhiteSpace(folder)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(folder));

			ITaskFolder actualFolder = _taskService.GetFolder(folder);
			actualFolder.DeleteTask(name, 0);
		}

		private void InstallOrUpdate(ITaskFolder folder, ITaskDefinition task, ScheduledTaskConfiguration configuration)
		{
			folder.RegisterTaskDefinition(
				Path: configuration.Name, 
				pDefinition: task, 
				flags: Convert.ToInt32(_TASK_CREATION.TASK_CREATE_OR_UPDATE), 
				UserId: configuration.Credentials.Username, 
				password: configuration.Credentials.Password,
				LogonType: configuration.GetLogonType());
		}

		private ITaskFolder GetOrCreateFolder(string folder)
		{
			if (folder == null) throw new ArgumentNullException(nameof(folder));

			ITaskFolder rootFolder = _taskService.GetFolder(@"\");
			IEnumerator subfolders = rootFolder.GetFolders(0).GetEnumerator();
			bool folderExists = false;
			ITaskFolder actualFolder = null;

			while (subfolders.MoveNext() && !folderExists)
			{
				actualFolder = (ITaskFolder)subfolders.Current;
				folderExists = actualFolder.Name.Equals(folder, StringComparison.OrdinalIgnoreCase);
			}

			if (!folderExists)
				actualFolder = rootFolder.CreateFolder(folder);

			return actualFolder;
		}
	}
}
