using System;
using Vertica.Integration.Redis.Infrastructure.Client;

namespace Vertica.Integration.Redis
{
	public class RedisConfiguration
	{
        private readonly RedisClientConfiguration _client;

        internal RedisConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

		    Application = application;

            _client = new RedisClientConfiguration(this);
            Application.Extensibility(extensibility => extensibility.Register(() => _client));
        }

	    public ApplicationConfiguration Application { get; }

	    public RedisConfiguration Change(Action<RedisConfiguration> change)
		{
			change?.Invoke(this);

			return this;
		}

        public RedisConfiguration Client(Action<RedisClientConfiguration> client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            client(_client);

            return this;
        }
    }
}