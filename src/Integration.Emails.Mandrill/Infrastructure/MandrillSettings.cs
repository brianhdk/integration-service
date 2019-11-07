using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Vertica.Integration.Emails.Mandrill.Infrastructure
{
    [Guid("C583D807-0878-4216-A75F-9DD2CB4D7EB4")]
    [Description("Configuration for Mandrill. Remember to restart the application if you modify this configuration.")]
    public class MandrillSettings
    {
        public Uri BaseUrl { get; set; } = new Uri("https://static.mandrillapp.com/api/1.0/");

        public string ApiKey { get; set; }

        internal void Validate()
        {
            if (BaseUrl == null)
                throw new InvalidOperationException($"Mandatory configuration value for '{nameof(BaseUrl)}' has not been specified.");

            if (string.IsNullOrWhiteSpace(ApiKey))
                throw new InvalidOperationException($"Mandatory configuration value for '{nameof(ApiKey)}' has not been specified.");
        }
    }
}