using System;
using System.Configuration;
using System.Linq;
using System.Net.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using Vertica.Integration.Infrastructure.Email;

namespace Vertica.Integration.Emails.SendGrid.Infrastructure
{
    internal class SendGridEmailService : IEmailService
    {
        private readonly SendGridSettings _settings;
        private readonly string _from;

        public SendGridEmailService(ISendGridSettingsProvider settingsProvider)
        {
            _settings = settingsProvider.Get();
            _settings.Validate();

            const string section = "system.net/mailSettings/smtp";

            SmtpSection settings = (SmtpSection) ConfigurationManager.GetSection(section);

            if (string.IsNullOrWhiteSpace(settings.From))
                throw new InvalidOperationException($"Attribute '{nameof(settings.From).ToLowerInvariant()}' has not been specified in config file '{settings.ElementInformation.Source}' in section '{section}'.");

            _from = settings.From;
        }

        public void Send(EmailTemplate template, params string[] recipients)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            if (recipients == null) throw new ArgumentNullException(nameof(recipients));
            if (recipients.Length == 0) throw new ArgumentException(@"Value cannot be an empty collection.", nameof(recipients));

            var client = new SendGridClient(_settings.ApiKey);

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(
                new EmailAddress(_from),
                recipients.Select(x => new EmailAddress(x)).ToList(),
                template.Subject,
                !template.IsHtml ? template.GetBody() : null,
                template.IsHtml ? template.GetBody() : null,
                true);

            client.SendEmailAsync(msg).Wait();
        }
    }
}