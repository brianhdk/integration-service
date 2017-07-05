using System;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Archiving;

namespace Vertica.Integration.Globase
{
    public class GlobaseConfiguration : IInitializable<ApplicationConfiguration>
	{
	    private readonly ConfigurationImpl _configuration;

		internal GlobaseConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

		    _configuration = new ConfigurationImpl();

            Application = application
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register<IGlobaseConfiguration>(kernel => _configuration))
                    .Conventions(conventions => conventions
                        .AddFromAssemblyOfThis<GlobaseConfiguration>()));
        }

		public GlobaseConfiguration FtpConnectionString(ConnectionString ftp)
		{
			if (ftp == null) throw new ArgumentNullException(nameof(ftp));

			_configuration.FtpConnectionString = ftp;

			return this;
		}

		public GlobaseConfiguration Change(Action<GlobaseConfiguration> change)
		{
			change?.Invoke(this);

			return this;
		}

		/// <summary>
		/// Will create an archive when uploading data to Globase.
		/// </summary>
		public GlobaseConfiguration EnableArchiving(Action<ArchiveOptions> options = null)
		{
			_configuration.ArchiveOptions = 
                new ArchiveOptions("Globase")
                    .GroupedBy("Globase")
                    .ExpiresAfterMonths(1);

			options?.Invoke(_configuration.ArchiveOptions);

			return this;
		}

		public ApplicationConfiguration Application { get; }

        internal class ConfigurationImpl : IGlobaseConfiguration
        {
            public ArchiveOptions ArchiveOptions { get; set; }
            public ConnectionString FtpConnectionString { get; set; }
        }

        void IInitializable<ApplicationConfiguration>.Initialized(ApplicationConfiguration application)
		{
			if (_configuration.FtpConnectionString == null)
				_configuration.FtpConnectionString = ConnectionString.FromName("Globase.Ftp.Url");
		}
	}
}