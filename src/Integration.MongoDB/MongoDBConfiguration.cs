using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.MongoDB.Infrastructure;
using Vertica.Integration.MongoDB.Infrastructure.Castle.Windsor;

namespace Vertica.Integration.MongoDB
{
    public class MongoDBConfiguration : IMongoDBConfiguration, IAdditionalConfiguration
    {
        private readonly List<IWindsorInstaller> _customInstallers;

        internal MongoDBConfiguration()
        {
            _customInstallers = new List<IWindsorInstaller>
            {
                new ConventionInstaller().AddFromAssemblyOfThis<MongoDBConfiguration>()
            };
        }

        internal IWindsorInstaller[] CustomInstallers
        {
            get { return _customInstallers.ToArray(); }
        }

        public IAdditionalConfiguration Connection(ConnectionString connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");

            _customInstallers.Add(new MongoDBInstaller(new DefaultConnection(connectionString)));

            return this;
        }

        public IAdditionalConfiguration AddConnection<TConnection>(TConnection connection)
            where TConnection : Connection
        {
            if (connection == null) throw new ArgumentNullException("connection");

            _customInstallers.Add(new MongoDBInstaller<TConnection>(connection));

            return this;
        }
    }
}