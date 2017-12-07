using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Vertica.Integration.Emails.SendGrid.Infrastructure
{
    [Guid("97EE1623-EC62-4DEB-87AD-D1BA4A037160")]
    [Description("Configuration for SendGrid. Remember to restart the application if you modify this configuration.")]
    public class SendGridSettings
    {
        public string ApiKey { get; set; }

        internal void Validate()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
                throw new InvalidOperationException($"Mandatory configuration value for '{nameof(ApiKey)}' has not been specified.");
        }
    }
}