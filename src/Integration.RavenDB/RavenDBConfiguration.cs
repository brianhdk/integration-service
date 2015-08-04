using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Vertica.Integration.RavenDB.Infrastructure;
using Vertica.Integration.RavenDB.Infrastructure.Castle.Windsor;

namespace Vertica.Integration.RavenDB
{
    public class RavenDbConfiguration : IInitializable<IWindsorContainer>
    {
		private IWindsorInstaller _defaultConnection;
        private readonly List<IWindsorInstaller> _connections;

		internal RavenDbConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException("application");

			Application = application.Extensibility(extensibility => extensibility.Register(this));

			_connections = new List<IWindsorInstaller>();
        }

        public ApplicationConfiguration Application { get; private set; }

		public RavenDbConfiguration DefaultConnection(Connection connection)
        {
			if (connection == null) throw new ArgumentNullException("connection");

			_defaultConnection = new RavenDbInstaller(new DefaultConnection(connection));

            return this;
        }

		public RavenDbConfiguration AddConnection<TConnection>(TConnection connection)
            where TConnection : Connection
        {
            if (connection == null) throw new ArgumentNullException("connection");

			_connections.Add(new RavenDbInstaller<TConnection>(connection));

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
	        if (_defaultConnection != null)
		        container.Install(_defaultConnection);

	        foreach (IWindsorInstaller installer in _connections)
		        container.Install(installer);
        }
    }
}