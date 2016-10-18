using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Database.Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Database
{
    public class DatabaseConfiguration : IInitializable<IWindsorContainer>
    {
        private readonly IntegrationDatabaseConfiguration _integrationDb;
        private readonly Dictionary<Type, IWindsorInstaller> _connections;

        internal DatabaseConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

            _integrationDb = new IntegrationDatabaseConfiguration(this);
            _connections = new Dictionary<Type, IWindsorInstaller>();

            Application = application
                .Extensibility(extensibility => extensibility
                    .Register(() => _integrationDb));
        }

        public ApplicationConfiguration Application { get; }

        public DatabaseConfiguration IntegrationDb(Action<IntegrationDatabaseConfiguration> integrationDb)
        {
            if (integrationDb == null) throw new ArgumentNullException(nameof(integrationDb));

            integrationDb(_integrationDb);

            return this;
        }

        [Obsolete("Use IntegrationDb(integrationDb => integrationDb.Disabled)")]
        public bool IntegrationDbDisabled => _integrationDb.Disabled;

        [Obsolete("Use IntegrationDb(integrationDb => integrationDb.Disable())")]
        public DatabaseConfiguration DisableIntegrationDb()
        {
            _integrationDb.Disable();

            return this;
        }

        [Obsolete("Use IntegrationDb(integrationDb => integrationDb.Connection(...))")]
        public DatabaseConfiguration IntegrationDb(ConnectionString connectionString)
	    {
            _integrationDb.Connection(connectionString);

		    return this;
	    }

        [Obsolete("Use IntegrationDb(integrationDb => integrationDb.Connection(...))")]
        public DatabaseConfiguration IntegrationDb(Connection connection)
	    {
            _integrationDb.Connection(connection);

            return this;
	    }

        /// <summary>
        /// Adds a custom database connection that can later be resolved by dependency <see cref="IDbFactory{TConnection}"/>.
        /// </summary>
        /// <typeparam name="TConnection">The specific connection to register.</typeparam>
	    public DatabaseConfiguration AddConnection<TConnection>(TConnection connection)
            where TConnection : Connection
	    {
	        _connections[typeof(TConnection)] = new DbInstaller<TConnection>(connection);

            return this;
        }

        /// <summary>
        /// Removes a custom database connection.
        /// </summary>
        /// <typeparam name="TConnection"></typeparam>
        /// <returns></returns>
        public DatabaseConfiguration RemoveConnection<TConnection>()
            where TConnection : Connection
        {
            _connections.Remove(typeof(TConnection));

            return this;
        }

        public DatabaseConfiguration Change(Action<DatabaseConfiguration> change)
        {
            change?.Invoke(this);

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
            foreach (IWindsorInstaller installer in _connections.Values)
                container.Install(installer);
        }
    }
}