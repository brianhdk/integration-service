using System;

namespace Vertica.Integration.Emails.SendGrid.Infrastructure.Providers
{
    internal class RuntimeSettingsSendGridSettingsProvider : ISendGridSettingsProvider
    {
        private readonly SendGridSettings _settings;

        public RuntimeSettingsSendGridSettingsProvider(IRuntimeSettings runtimeSettings)
        {
            if (runtimeSettings == null) throw new ArgumentNullException(nameof(runtimeSettings));

            _settings = new SendGridSettings
            {
                ApiKey = runtimeSettings["SendGrid.ApiKey"]
            };
        }

        public SendGridSettings Get()
        {
            return _settings;
        }
    }
}