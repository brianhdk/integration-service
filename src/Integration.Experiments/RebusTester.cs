using Microsoft.Azure;
using Rebus.Config;
using Vertica.Integration.Rebus;

namespace Vertica.Integration.Experiments
{
	public static class RebusTester
	{
		public static ApplicationConfiguration TestRebus(this ApplicationConfiguration application)
		{
			string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

			return application.UseRebus(rebus => rebus.Bus(bus => bus
				.Transport(t => t.UseAzureServiceBus(connectionString, "InputQueue"))));
		}
	}
}