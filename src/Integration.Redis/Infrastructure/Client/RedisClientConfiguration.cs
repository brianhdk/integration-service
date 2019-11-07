using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Redis.Infrastructure.Client.Castle.Windsor;

namespace Vertica.Integration.Redis.Infrastructure.Client
{
	public class RedisClientConfiguration : IInitializable<ApplicationConfiguration>
	{
		private IWindsorInstaller _defaultConnection;
		private readonly List<IWindsorInstaller> _connections;

		internal RedisClientConfiguration(RedisConfiguration redis)
        {
            if (redis == null) throw new ArgumentNullException(nameof(redis));

		    _connections = new List<IWindsorInstaller>();

		    Redis = redis;
        }

        public RedisConfiguration Redis { get; }

		public RedisClientConfiguration DefaultConnection(ConnectionString connectionString)
		{
			if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

			_defaultConnection = new RedisClientInstaller(new DefaultConnection(connectionString));

			return this;
		}

		public RedisClientConfiguration DefaultConnection(Connection connection)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_defaultConnection = new RedisClientInstaller(new DefaultConnection(connection));

			return this;
		}

		public RedisClientConfiguration AddConnection<TConnection>(TConnection connection)
			where TConnection : Connection
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_connections.Add(new RedisClientInstaller<TConnection>(connection));

			return this;
		}

        void IInitializable<ApplicationConfiguration>.Initialized(ApplicationConfiguration application)
        {
            application.Services(services => services.Advanced(advanced =>
            {
                if (_defaultConnection == null)
                    DefaultConnection(ConnectionString.FromName("Redis"));

                if (_defaultConnection != null)
                    advanced.Install(_defaultConnection);

                advanced.Install(_connections.ToArray());
            }));
        }
    }
}