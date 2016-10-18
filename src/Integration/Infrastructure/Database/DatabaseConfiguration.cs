using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Database.Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Infrastructure.Database
{
    public class DatabaseConfiguration : IDatabaseConfiguration, IInitializable<IWindsorContainer>
    {
        private DefaultConnection _defaultConnection;
        private readonly Dictionary<Type, IWindsorInstaller> _customConnections;

        internal DatabaseConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

            _customConnections = new Dictionary<Type, IWindsorInstaller>();

            Application = application;
        }

        public ApplicationConfiguration Application { get; }

        public bool IntegrationDbDisabled { get; private set; }
        //public Func<IKernel, string> IntegrationDbTablePrefix { get; private set; }

        public DatabaseConfiguration DisableIntegrationDb()
        {
            IntegrationDbDisabled = true;

            return this;
        }

        //public DatabaseConfiguration PrefixIntegrationDbTables(Func<IKernel, string> prefix)
        //{
        //    if (prefix == null) throw new ArgumentNullException(nameof(prefix));

        //    IntegrationDbTablePrefix = prefix;

        //    return this;
        //}

        /// <summary>
        /// Specifies the connection string for the IntegrationDb.
        /// </summary>
        public DatabaseConfiguration IntegrationDb(ConnectionString connectionString)
	    {
		    if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

		    _defaultConnection = new DefaultConnection(connectionString);

		    return this;
	    }

        /// <summary>
        /// Specifies a specific connection instance for the IntegrationDb.
        /// </summary>
	    public DatabaseConfiguration IntegrationDb(Connection connection)
	    {
		    if (connection == null) throw new ArgumentNullException(nameof(connection));

            _defaultConnection = new DefaultConnection(connection);

			return this;
	    }

        /// <summary>
        /// Adds a custom database connection that can later be resolved by dependency <see cref="IDbFactory{TConnection}"/>.
        /// </summary>
        /// <typeparam name="TConnection">The specific connection to register.</typeparam>
	    public DatabaseConfiguration AddConnection<TConnection>(TConnection connection)
            where TConnection : Connection
	    {
	        _customConnections[typeof(TConnection)] = new DbInstaller<TConnection>(connection);

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
            _customConnections.Remove(typeof(TConnection));

            return this;
        }

        public DatabaseConfiguration Change(Action<DatabaseConfiguration> change)
        {
            change?.Invoke(this);

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
            container.RegisterInstance<IDatabaseConfiguration>(this, x => x.LifestyleSingleton());

	        container.Install(new DbInstaller(IntegrationDbDisabled ? 
				DefaultConnection.Disabled : 
				_defaultConnection ?? new DefaultConnection(ConnectionString.FromName("IntegrationDb"))));

            foreach (IWindsorInstaller installer in _customConnections.Values)
                container.Install(installer);
        }
    }
}