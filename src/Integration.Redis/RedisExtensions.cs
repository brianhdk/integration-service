using System;

namespace Vertica.Integration.Redis
{
	public static class RedisExtensions
	{
		public static ApplicationConfiguration UseRedis(this ApplicationConfiguration application, Action<RedisConfiguration> redis = null)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			return application.Extensibility(extensibility =>
			{
				RedisConfiguration configuration = extensibility.Register(() => new RedisConfiguration(application));

				redis?.Invoke(configuration);
			});
		}
	}
}