using System;
using Vertica.Integration;

namespace $rootnamespace$
{
	public static class IntegrationStartup
	{
		public static void Run(string[] args, Action<ApplicationConfiguration> application = null)
		{
			using (ApplicationContext context = ApplicationContext.Create(cfg => cfg.Change(application)))
			{
				context.Execute(args);
			}
		}
	}
}