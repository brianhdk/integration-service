using Vertica.Integration.Infrastructure.Configuration;

namespace Vertica.Integration.Emails.Mandrill.Infrastructure.Providers
{
    internal class ConfigurationServiceMandrillSettingsProvider : IMandrillSettingsProvider
    {
        private readonly MandrillSettings _settings;

        public ConfigurationServiceMandrillSettingsProvider(IConfigurationService configurationService)
        {
            _settings = configurationService.Get<MandrillSettings>();
        }

        public MandrillSettings Get()
        {
            return _settings;
        }
    }
}