using System;
using Vertica.Integration.Infrastructure.Configuration;

namespace Vertica.Integration.Domain.LiteServer.Heartbeat.Logging
{
    internal class ConfigurationServiceBasedHeartbeatLoggingRepository : IHeartbeatLoggingRepository
    {
        private readonly IConfigurationService _service;
        private readonly IRuntimeSettings _settings;
        private readonly string _machineName;

        public ConfigurationServiceBasedHeartbeatLoggingRepository(IConfigurationService service, IRuntimeSettings settings)
        {
            _service = service;
            _settings = settings;
            _machineName = Environment.MachineName;
        }

        public void Insert(HeartbeatLogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            var configuration = _service.Get<HeartbeatLog>();

            configuration.Cleanup(
                (int)ReadMaximumEntries().GetValueOrDefault(500u),
                ReadMaximumAge().GetValueOrDefault(TimeSpan.FromDays(14)));

            configuration.Add(_machineName, entry);

            _service.Save(configuration, GetType().Name);
        }

        private uint? ReadMaximumEntries()
        {
            string key = $"{nameof(ConfigurationServiceBasedHeartbeatLoggingRepository)}.MaximumNumberOfEntries";

            string value = _settings[key];

            if (string.IsNullOrWhiteSpace(value))
                return null;

            uint result;
            if (!uint.TryParse(value, out result) || result == 0)
                throw new InvalidOperationException($@"Invalid value '{value}' to be used for setting a maximum number of entries to log. 

Please specify the value for '{key}' to a positive number.");

            return result;
        }

        private TimeSpan? ReadMaximumAge()
        {
            string key = $"{nameof(ConfigurationServiceBasedHeartbeatLoggingRepository)}.MaximumAge";

            string value = _settings[key];

            if (string.IsNullOrWhiteSpace(value))
                return null;

            TimeSpan result;
            if (!TimeSpan.TryParse(value, out result) || result.TotalSeconds <= 0)
                throw new InvalidOperationException($@"Invalid value '{value}' to be used for setting a maximum age on logged entries. 

Please specify the value for '{key}' to a positive TimeSpan.");

            return result;
        }
    }
}