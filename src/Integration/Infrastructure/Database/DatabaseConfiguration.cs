using System;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Database.Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Database
{
    public class DatabaseConfiguration : IInitializable<IWindsorContainer>
    {
	    private DefaultConnection _defaultConnection;

        internal DatabaseConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

	        Application = application;
        }

        public ApplicationConfiguration Application { get; }

        public bool IntegrationDbDisabled { get; private set; }

        public DatabaseConfiguration DisableIntegrationDb()
        {
            IntegrationDbDisabled = true;
            return this;
        }

	    public DatabaseConfiguration IntegrationDb(ConnectionString connectionString)
	    {
		    if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

		    _defaultConnection = new DefaultConnection(connectionString);

		    return this;
	    }

	    public DatabaseConfiguration IntegrationDb(Connection connection)
	    {
		    if (connection == null) throw new ArgumentNullException(nameof(connection));

		    _defaultConnection = new DefaultConnection(connection);

			return this;
	    }

	    public DatabaseConfiguration AddConnection<TConnection>(TConnection connection)
            where TConnection : Connection
        {
            Application.AddCustomInstaller(new DbInstaller<TConnection>(connection));

            return this;
        }

        public DatabaseConfiguration Change(Action<DatabaseConfiguration> change)
        {
            if (change != null)
                change(this);

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
	        container.Install(new DbInstaller(IntegrationDbDisabled ? 
				DefaultConnection.Disabled : 
				_defaultConnection ?? new DefaultConnection(ConnectionString.FromName("IntegrationDb"))));
        }
    }
}