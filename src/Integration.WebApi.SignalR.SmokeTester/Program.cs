using Vertica.Integration;
using Vertica.Integration.WebApi;
using Vertica.Integration.WebApi.SignalR;

namespace Integration.WebApi.SignalR.SmokeTester
{
	class Program
	{
		static void Main()
		{
			using (IApplicationContext context = ApplicationContext.Create(application => application
				.Database(database => database.DisableIntegrationDb())
				.UseWebApi(webApi => webApi
					.Clear().AddFromAssemblyOfThis<Program>()
					.WithSignalR(signalR => signalR.AddFromAssemblyOfThis<Program>()))))
			{
				context.Execute("WebApiHost");
			}
		}
	}
}