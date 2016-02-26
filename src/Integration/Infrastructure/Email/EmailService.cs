using System;
using System.Net.Mail;

namespace Vertica.Integration.Infrastructure.Email
{
	public class EmailService : IEmailService
	{
		public void Send(EmailTemplate template, params string[] recipients)
		{
			if (template == null) throw new ArgumentNullException(nameof(template));
		    if (recipients == null) throw new ArgumentNullException(nameof(recipients));

		    using (var message = new MailMessage())
			using (var smtpClient = new SmtpClient())
			{
				message.Subject = template.Subject;
				message.Body = template.GetBody();
				message.IsBodyHtml = template.IsHtml;

			    if (template.MailPriority.HasValue)
			        message.Priority = template.MailPriority.Value;

				foreach (string recipient in recipients)
				{
					if (!string.IsNullOrWhiteSpace(recipient))
						message.To.Add(recipient.Trim());
				}

			    foreach (Attachment attachment in template.Attachments ?? new Attachment[0])
			        message.Attachments.Add(attachment);

				smtpClient.Send(message);
			}
		}
	}
}