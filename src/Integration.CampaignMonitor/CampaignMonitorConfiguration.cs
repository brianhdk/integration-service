using System;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.CampaignMonitor
{
	public class CampaignMonitorConfiguration : IInitializable<IWindsorContainer>
	{
		internal CampaignMonitorConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			Application = application
				.AddCustomInstaller(Install.ByConvention
					.AddFromAssemblyOfThis<CampaignMonitorConfiguration>()
					.Ignore<CampaignMonitorConfiguration>());
		}

		public ApplicationConfiguration Application { get; private set; }

		void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			container.RegisterInstance(this);
		}
	}
}