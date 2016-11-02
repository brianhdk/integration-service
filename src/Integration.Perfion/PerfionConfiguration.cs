using System;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Archiving;

namespace Vertica.Integration.Perfion
{
    public class PerfionConfiguration : IInitializable<ApplicationConfiguration>
    {
        private readonly ConfigurationImpl _configuration;

		internal PerfionConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

            _configuration = new ConfigurationImpl();

            Application = application
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register<IPerfionConfiguration>(kernel => _configuration))
                    .Conventions(conventions => conventions
                        .AddFromAssemblyOfThis<PerfionConfiguration>()));
		}

        public ApplicationConfiguration Application { get; private set; }

		public PerfionConfiguration ConnectionString(ConnectionString connectionString)
		{
			if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            _configuration.ConnectionString = connectionString;

			return this;
		}

		/// <summary>
		/// ServiceClient allows configuration of the WCF proxy client that connects to the Perfion API.
		/// </summary>
		public PerfionConfiguration ServiceClient(Action<ServiceClientConfiguration> serviceClient)
		{
			if (serviceClient == null) throw new ArgumentNullException(nameof(serviceClient));

			serviceClient(_configuration.ServiceClientConfiguration);

			return this;
		}

		/// <summary>
		/// WebClient is used for all download related tasks, e.g. downloading files, images and reports.
		/// </summary>
		public PerfionConfiguration WebClient(Action<WebClientConfiguration> webClient)
		{
			if (webClient == null) throw new ArgumentNullException(nameof(webClient));

			webClient(_configuration.WebClientConfiguration);

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
			_configuration.ArchiveOptions = 
                new ArchiveOptions("Data")
                    .GroupedBy("Perfion")
                    .ExpiresAfterMonths(1);

			options?.Invoke(_configuration.ArchiveOptions);

			return this;
		}

        internal class ConfigurationImpl : IPerfionConfiguration
        {
            public ConfigurationImpl()
            {
                ServiceClientConfiguration = new ServiceClientConfiguration();
                WebClientConfiguration = new WebClientConfiguration();
            }

            public ConnectionString ConnectionString { get; set; }
            public ArchiveOptions ArchiveOptions { get; set; }
            public ServiceClientConfiguration ServiceClientConfiguration { get; }
            public WebClientConfiguration WebClientConfiguration { get; }
        }

        void IInitializable<ApplicationConfiguration>.Initialized(ApplicationConfiguration application)
		{
			if (_configuration.ConnectionString == null)
				_configuration.ConnectionString = Integration.Infrastructure.ConnectionString.FromName("Perfion.APIService.Url");
		}
	}
}