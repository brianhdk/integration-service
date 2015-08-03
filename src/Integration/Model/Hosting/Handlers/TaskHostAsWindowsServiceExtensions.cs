using System;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public static class TaskHostAsWindowsServiceExtensions
	{
		private const string RepeatCommand = "repeat";

		public static InstallWindowsService Repeat(this InstallWindowsService install, TimeSpan repeat)
		{
			if (install == null) throw new ArgumentNullException("install");

			return install.CustomCommandArgument(RepeatCommand, repeat.ToString());
		}

		internal static TimeSpan ParseRepeat(this HostArguments args)
		{
			if (args == null) throw new ArgumentNullException("args");

			string value;
			args.CommandArgs.TryGetValue(RepeatCommand, out value);

			TimeSpan repeat;
			if (!TimeSpan.TryParse(value, out repeat))
				repeat = TimeSpan.FromMinutes(1);

			return repeat;
		}
	}
}