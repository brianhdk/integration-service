using System;
using System.Collections;
using System.Collections.Generic;
using TaskScheduler;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	internal class ScheduleTaskInstaller 
	{
		private readonly ITaskService _taskService;

		public ScheduleTaskInstaller()
		{
			_taskService = new TaskScheduler.TaskScheduler();
			_taskService.Connect();
		}

		public void InstallOrUpdate(ScheduleTask scheduleTask)
		{
			if (scheduleTask == null)
				throw new ArgumentNullException("scheduleTask");

			ITaskDefinition taskDefinition = CreateTaskDefinition(_taskService, scheduleTask);
			AddActions(taskDefinition, scheduleTask.Actions);
			InstallOrUpdate(taskDefinition, scheduleTask);
		}

		public void Uninstall(ScheduleTask scheduleTask)
		{
			Uninstall(scheduleTask.Name, scheduleTask.FolderName);
		}

		public void Uninstall(string scheduleTaskName, string folderName)
		{
			ITaskFolder folder = _taskService.GetFolder(folderName);
			folder.DeleteTask(scheduleTaskName, 0);
		}

		private void InstallOrUpdate(ITaskDefinition taskDefinition, ScheduleTask scheduleTask)
		{
			if(scheduleTask.Credentials == null)
				throw new InvalidOperationException("No user defined in parameter scheduleTask for running this task");

			ITaskFolder folder = GetOrCreateFolder(scheduleTask.FolderName);
			folder.RegisterTaskDefinition(
				Path: scheduleTask.Name, 
				pDefinition: taskDefinition, 
				flags: Convert.ToInt32(_TASK_CREATION.TASK_CREATE_OR_UPDATE), 
				UserId: scheduleTask.Credentials.Username, 
				password: scheduleTask.Credentials.Password, 
				LogonType: _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN_OR_PASSWORD);
		}

		private ITaskFolder GetOrCreateFolder(string folderName)
		{
			if (folderName == null) throw new ArgumentNullException("folderName");

			ITaskFolder rootFolder = _taskService.GetFolder(@"\");
			IEnumerator subfolders = rootFolder.GetFolders(0).GetEnumerator();
			bool folderExists = false;

			ITaskFolder folder = null;

			while (subfolders.MoveNext() && !folderExists)
			{
				folder = ((ITaskFolder)subfolders.Current);
				folderExists = folder.Name.Equals(folderName, StringComparison.OrdinalIgnoreCase);
			}

			if (!folderExists)
				folder = rootFolder.CreateFolder(folderName);

			return folder;
		}

		private static void AddActions(ITaskDefinition taskDefinition, IEnumerable<Action> actions)
		{
			foreach (var action in taskDefinition.Actions)
			{
				taskDefinition.Actions.Remove(action);
			}

			foreach (var action in actions)
			{
				IExecAction taskAction = (IExecAction)taskDefinition.Actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
				taskAction.Path = action.ExePath;
				taskAction.Arguments = action.Arguments ?? String.Empty;
				taskAction.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}
		}

		private static ITaskDefinition CreateTaskDefinition(ITaskService taskService, ScheduleTask scheduleTask)
		{
			ITaskDefinition taskDefinition = taskService.NewTask(0);
			taskDefinition.Settings.MultipleInstances = _TASK_INSTANCES_POLICY.TASK_INSTANCES_IGNORE_NEW;
			taskDefinition.RegistrationInfo.Description = scheduleTask.Description;
			taskDefinition.Principal.LogonType = _TASK_LOGON_TYPE.TASK_LOGON_GROUP;
			taskDefinition.Settings.Hidden = false;
			return taskDefinition;
		}
	}
}
