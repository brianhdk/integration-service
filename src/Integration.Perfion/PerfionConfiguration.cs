using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Perfion.Infrastructure.Client;
using Vertica.Integration.Perfion.Infrastructure.Client.Castle.Windsor;

namespace Vertica.Integration.Perfion
{
    public class PerfionConfiguration : IInitializable<ApplicationConfiguration>
    {
        private IWindsorInstaller _defaultConnection;
        private readonly List<IWindsorInstaller> _connections;

        internal PerfionConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

            Application = application;

            _connections = new List<IWindsorInstaller>();
        }
        
        public ApplicationConfiguration Application { get; }

        public PerfionConfiguration DefaultConnection(ConnectionString connectionString, Action<PerfionClientConfiguration> perfionClient = null)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            var configuration = new PerfionClientConfiguration();
            perfionClient?.Invoke(configuration);

            _defaultConnection = new PerfionClientInstaller(
                new DefaultConnection(connectionString),
                configuration.Map());

            return this;
        }

        public PerfionConfiguration DefaultConnection(Connection connection, Action<PerfionClientConfiguration> perfionClient = null)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var configuration = new PerfionClientConfiguration();
            perfionClient?.Invoke(configuration);

            _defaultConnection = new PerfionClientInstaller(
                new DefaultConnection(connection), 
                configuration.Map());
            
            return this;
        }

        public PerfionConfiguration AddConnection<TConnection>(TConnection connection, Action<PerfionClientConfiguration> perfionClient = null)
            where TConnection : Connection
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var configuration = new PerfionClientConfiguration();
            perfionClient?.Invoke(configuration);

            _connections.Add(new PerfionClientInstaller<TConnection>(
                connection, 
                configuration.Map()));

            return this;
        }

        void IInitializable<ApplicationConfiguration>.Initialized(ApplicationConfiguration application)
        {
            application.Services(services => services.Advanced(advanced =>
            {
                if (_defaultConnection == null)
                    DefaultConnection(ConnectionString.FromName("Perfion.APIService.Url"));

                if (_defaultConnection != null)
                    advanced.Install(_defaultConnection);

                advanced.Install(_connections.ToArray());
            }));
        }
    }
}