﻿namespace Vertica.Integration.Model.Hosting.Handlers
{
	public interface IScheduleTaskHandler
	{
		bool Handle(ITask task, HostArguments args);
	}
}