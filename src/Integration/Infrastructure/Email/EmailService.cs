using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace Vertica.Integration.Infrastructure.Email
{
	public class EmailService : IEmailService
	{
		public void Send(EmailTemplate template, IEnumerable<string> recipients)
		{
			if (template == null) throw new ArgumentNullException("template");

			using (var message = new MailMessage())
			using (var smtpClient = new SmtpClient())
			{
				message.Subject = template.Subject;
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