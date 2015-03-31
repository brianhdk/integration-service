using System;
using Vertica.Integration.Infrastructure.Database.Dapper.Castle.Windsor;
using Vertica.Integration.Infrastructure.Database.Dapper.Databases;

namespace Vertica.Integration.Infrastructure.Database.Dapper
{
    public class DapperConfiguration
    {
        private readonly ApplicationConfiguration _configuration;

        internal DapperConfiguration(ApplicationConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            _configuration = configuration;

            _configuration.AddCustomInstaller(
                new DapperInstaller(new IntegrationDb(configuration.DatabaseConnectionString)));
        }

        public DapperConfiguration AddConnection<TConnection>(TConnection connection)
            where TConnection : Connection
        {
            _configuration.AddCustomInstaller(
                new DapperInstaller<TConnection>(connection));

            return this;
        }
    }
}