using System;
using Castle.Windsor;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Perfion
{
	public class PerfionConfiguration : IInitializable<IWindsorContainer>
	{
		internal PerfionConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			Application = application
				.AddCustomInstaller(Install.ByConvention
					.AddFromAssemblyOfThis<PerfionConfiguration>()
					.Ignore<PerfionConfiguration>());

			ServiceClientConfiguration = new ServiceClientConfiguration();
			WebClientConfiguration = new WebClientConfiguration();
		}

		internal ConnectionString ConnectionStringInternal { get; set; }
		internal ArchiveOptions ArchiveOptions { get; private set; }
		internal ServiceClientConfiguration ServiceClientConfiguration { get; }
		internal WebClientConfiguration WebClientConfiguration { get; }

		public PerfionConfiguration ConnectionString(ConnectionString connectionString)
		{
			if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

			ConnectionStringInternal = connectionString;

			return this;
		}

		/// <summary>
		/// ServiceClient allows configuration of the WCF proxy client that connects to the Perfion API.
		/// </summary>
		public PerfionConfiguration ServiceClient(Action<ServiceClientConfiguration> serviceClient)
		{
			if (serviceClient == null) throw new ArgumentNullException(nameof(serviceClient));

			serviceClient(ServiceClientConfiguration);

			return this;
		}

		/// <summary>
		/// WebClient is used for all download related tasks, e.g. downloading files, images and reports.
		/// </summary>
		public PerfionConfiguration WebClient(Action<WebClientConfiguration> webClient)
		{
			if (webClient == null) throw new ArgumentNullException(nameof(webClient));

			webClient(WebClientConfiguration);

			return this;
		}

		public PerfionConfiguration Change(Action<PerfionConfiguration> change)
		{
			change?.Invoke(this);

			return this;
		}

		/// <summary>
		/// Will create an archive when querying for data in Perfion.
		/// </summary>
		public PerfionConfiguration EnableArchiving(Action<ArchiveOptions> options = null)
		{
			ArchiveOptions = new ArchiveOptions("Data").GroupedBy("Perfion").ExpiresAfterMonths(1);

			options?.Invoke(ArchiveOptions);

			return this;
		}

		public ApplicationConfiguration Application { get; private set; }

		void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			if (ConnectionStringInternal == null)
				ConnectionStringInternal = Integration.Infrastructure.ConnectionString.FromName("Perfion.APIService.Url");

			container.RegisterInstance(this);
		}
	}
}