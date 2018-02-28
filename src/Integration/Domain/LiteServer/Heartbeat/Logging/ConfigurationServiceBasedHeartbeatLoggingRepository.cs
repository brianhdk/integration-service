using System;
using System.Collections.Generic;
using Vertica.Integration.Infrastructure.Configuration;

namespace Vertica.Integration.Domain.LiteServer.Heartbeat.Logging
{
    internal class ConfigurationServiceBasedHeartbeatLoggingRepository : IHeartbeatLoggingRepository
    {
        private readonly IConfigurationService _service;
        private readonly IRuntimeSettings _settings;

        public ConfigurationServiceBasedHeartbeatLoggingRepository(IConfigurationService service, IRuntimeSettings settings)
        {
            _service = service;
            _settings = settings;
        }

        public void Insert(HeartbeatLogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            var configuration = _service.Get<HeartbeatLog>();

            List<HeartbeatLogEntry> entries = configuration.Entries;

            entries.Add(entry);
            entries.Sort();

            int maxCount = (int) ReadMaximumEntries().GetValueOrDefault(500);

            if (entries.Count > maxCount)
                entries.RemoveRange(maxCount, entries.Count - maxCount);

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
    }
}