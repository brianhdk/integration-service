using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Configuration;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Vertica.Integration.Infrastructure.Email;

namespace Vertica.Integration.Emails.Mandrill.Infrastructure
{
    internal class MandrillEmailService : IEmailService
    {
        private readonly IMandrillApiService _api;
        private readonly string _from;

        public MandrillEmailService(IMandrillApiService api)
        {
            _api = api;

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

            var request = new SendEmailRequest(template, recipients, _from);

            Task response = _api.RequestAsync("messages/send", request);

            response.Wait();
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
        private class SendEmailRequest : MandrillApiRequest
        {
            private readonly EmailTemplate _email;
            private readonly string[] _recipients;
            private readonly string _from;

            public SendEmailRequest(EmailTemplate email, string[] recipients, string from)
            {
                _email = email;
                _recipients = recipients;
                _from = from;
            }

            [JsonProperty("message")]
            public MessageRequest Message => new MessageRequest(_email, _recipients, _from);

            public class MessageRequest
            {
                private readonly EmailTemplate _email;
                private readonly string[] _recipients;
                private readonly string _from;

                public MessageRequest(EmailTemplate email, string[] recipients, string from)
                {
                    _email = email;
                    _recipients = recipients;
                    _from = from;
                }

                [JsonProperty("text")]
                public string Text => !_email.IsHtml ? _email.GetBody() : null;

                [JsonProperty("html")]
                public string Html => _email.IsHtml ? _email.GetBody() : null;

                [JsonProperty("subject")]
                public string Subject => _email.Subject;

                [JsonProperty("from_email")]
                public string FromEmail => _from;

                [JsonProperty("to")]
                public Recipient[] To => _recipients.Select(x => new Recipient(x)).ToArray();
                
                public class Recipient
                {
                    private readonly string _email;

                    public Recipient(string email)
                    {
                        _email = email;
                    }

                    [JsonProperty("email")]
                    public string Email => _email;
                }
            }
        }
    }
}