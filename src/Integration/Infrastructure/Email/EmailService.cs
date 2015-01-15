using System;
using System.Collections.Generic;
using System.Net.Mail;
using Vertica.Integration.Properties;

namespace Vertica.Integration.Infrastructure.Email
{
	public class EmailService : IEmailService
	{
		private readonly ISettings _settings;

		public EmailService(ISettings settings)
		{
			_settings = settings;
		}

		public void Send(EmailTemplate template, IEnumerable<string> recipients)
		{
			if (template == null) throw new ArgumentNullException("template");

			using (var message = new MailMessage())
			using (var smtpClient = new SmtpClient())
			{
				message.Subject = String.Format(_settings.EmailSubjectFormat, template.Subject);
				message.Body = template.GetBody();
				message.IsBodyHtml = template.IsHtml;

				foreach (var recipient in recipients)
				{
					if (!String.IsNullOrWhiteSpace(recipient))
						message.To.Add(recipient.Trim());
				}

				smtpClient.Send(message);
			}
		}
	}
}