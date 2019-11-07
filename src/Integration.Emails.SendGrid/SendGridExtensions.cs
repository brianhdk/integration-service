using System;

namespace Vertica.Integration.Emails.SendGrid
{
	public static class SendGridExtensions
	{
		public static ApplicationConfiguration UseSendGrid(this ApplicationConfiguration application, Action<SendGridConfiguration> SendGrid = null)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			return application.Extensibility(extensibility =>
			{
			    SendGridConfiguration configuration = extensibility.Register(() => new SendGridConfiguration(application));

				SendGrid?.Invoke(configuration);
			});
		}
    }
}