using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Slack;

namespace Experiments.Slack
{
	class Program
	{
		static void Main(string[] args)
		{
			using (IApplicationContext context = ApplicationContext.Create(application => application
				.UseSlack(slack => slack
					.AddToLiteServer())))
			{
				context.Execute(typeof(LiteServerHost).HostName());
			}
		}
	}
}
