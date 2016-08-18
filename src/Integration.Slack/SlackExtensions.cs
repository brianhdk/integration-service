using System;

namespace Vertica.Integration.Slack
{
	public static class SlackExtensions
	{
		public static ApplicationConfiguration UseSlack(this ApplicationConfiguration application, Action<SlackConfiguration> slack = null)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			return application.Extensibility(extensibility =>
			{
				SlackConfiguration configuration = extensibility.Register(() => new SlackConfiguration(application));

				slack?.Invoke(configuration);
			});
		}
	}
}