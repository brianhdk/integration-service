using System;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Model;
using Vertica.Integration.Redis.Infrastructure.Client;
using Vertica.Integration.Redis.Monitor;

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
        
	    public static TaskConfiguration<MonitorWorkItem> IncludeRedis(this TaskConfiguration<MonitorWorkItem> task)
	    {
	        if (task == null) throw new ArgumentNullException(nameof(task));

	        return task.Step<PingRedisStep>();
	    }

	    public static TaskConfiguration<MonitorWorkItem> IncludeRedis<TConnection>(this TaskConfiguration<MonitorWorkItem> task)
	        where TConnection : Connection
	    {
	        return task.Step<PingRedisStep<TConnection>>();
	    }
    }
}