using System;

namespace Vertica.Integration.CampaignMonitor
{
	public static class CampaignMonitorExtensions
	{
		public static ApplicationConfiguration UseCampaignMonitor(this ApplicationConfiguration application, Action<CampaignMonitorConfiguration> campaignMonitor = null)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			return application.Extensibility(extensibility =>
			{
				CampaignMonitorConfiguration configuration = extensibility.Register(() => new CampaignMonitorConfiguration(application));

				campaignMonitor?.Invoke(configuration);
			});
		}
	}
}