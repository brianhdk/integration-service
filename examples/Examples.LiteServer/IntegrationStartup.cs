using System;
using Vertica.Integration;

namespace Examples.LiteServer
{
	public static class IntegrationStartup
	{
		public static void Run(string[] args, Action<ApplicationConfiguration> application = null)
		{
			using (IApplicationContext context = ApplicationContext.Create(cfg => cfg.Change(application)))
			{
				context.Execute(args);
			}
		}
	}
}