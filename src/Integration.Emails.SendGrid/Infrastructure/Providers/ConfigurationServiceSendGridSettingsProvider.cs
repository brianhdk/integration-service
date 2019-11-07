using Vertica.Integration.Infrastructure.Configuration;

namespace Vertica.Integration.Emails.SendGrid.Infrastructure.Providers
{
    internal class ConfigurationServiceSendGridSettingsProvider : ISendGridSettingsProvider
    {
        private readonly SendGridSettings _settings;

        public ConfigurationServiceSendGridSettingsProvider(IConfigurationService configurationService)
        {
            _settings = configurationService.Get<SendGridSettings>();
        }

        public SendGridSettings Get()
        {
            return _settings;
        }
    }
}