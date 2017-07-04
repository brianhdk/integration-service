using System;
using Vertica.Integration.Domain.LiteServer;

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

	    public static LiteServerConfiguration AddSlack(this LiteServerConfiguration liteServer)
	    {
	        if (liteServer == null) throw new ArgumentNullException(nameof(liteServer));

	        UseSlack(liteServer.Application, slack => slack.AddToLiteServer());

	        return liteServer;
	    }
    }
}