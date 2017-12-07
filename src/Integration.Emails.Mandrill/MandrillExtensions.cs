using System;

namespace Vertica.Integration.Emails.Mandrill
{
	public static class MandrillExtensions
	{
		public static ApplicationConfiguration UseMandrill(this ApplicationConfiguration application, Action<MandrillConfiguration> mandrill = null)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			return application.Extensibility(extensibility =>
			{
			    MandrillConfiguration configuration = extensibility.Register(() => new MandrillConfiguration(application));

				mandrill?.Invoke(configuration);
			});
		}
    }
}