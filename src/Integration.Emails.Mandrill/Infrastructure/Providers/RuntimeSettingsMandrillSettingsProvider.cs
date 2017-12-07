using System;

namespace Vertica.Integration.Emails.Mandrill.Infrastructure.Providers
{
    internal class RuntimeSettingsMandrillSettingsProvider : IMandrillSettingsProvider
    {
        private readonly MandrillSettings _settings;

        public RuntimeSettingsMandrillSettingsProvider(IRuntimeSettings runtimeSettings)
        {
            if (runtimeSettings == null) throw new ArgumentNullException(nameof(runtimeSettings));

            _settings = new MandrillSettings
            {
                ApiKey = runtimeSettings["Mandrill.ApiKey"]
            };

            Uri baseUrl;
            if (Uri.TryCreate(runtimeSettings["Mandrill.BaseUrl"], UriKind.RelativeOrAbsolute, out baseUrl))
                _settings.BaseUrl = baseUrl;
        }

        public MandrillSettings Get()
        {
            return _settings;
        }
    }
}