using System;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Perfion.Infrastructure.Client;

namespace Vertica.Integration.Perfion
{
    public class PerfionClientConfiguration
    {
        private readonly ConfigurationImpl _configuration;

        internal PerfionClientConfiguration()
        {
            _configuration = new ConfigurationImpl();
        }

		/// <summary>
		/// Will create an archive when querying for data in Perfion.
		/// </summary>
		public PerfionClientConfiguration EnableArchiving(Action<ArchiveOptions> options = null)
		{
			_configuration.ArchiveOptions = 
                new ArchiveOptions("Data")
                    .GroupedBy("Perfion")
                    .ExpiresAfterMonths(1);

			options?.Invoke(_configuration.ArchiveOptions);

			return this;
		}

        internal IPerfionClientConfiguration Map()
        {
            return _configuration;
        }

        private class ConfigurationImpl : IPerfionClientConfiguration
        {
            public ArchiveOptions ArchiveOptions { get; set; }
        }
	}
}