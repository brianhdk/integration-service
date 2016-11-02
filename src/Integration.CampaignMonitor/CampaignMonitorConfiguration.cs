using System;

namespace Vertica.Integration.CampaignMonitor
{
	public class CampaignMonitorConfiguration
	{
		internal CampaignMonitorConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

            Application = application
                .Services(services => services
                    .Conventions(conventions => conventions
                        .AddFromAssemblyOfThis<CampaignMonitorConfiguration>()));
        }

		public ApplicationConfiguration Application { get; private set; }
    }
}