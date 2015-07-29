using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.MongoDB.Infrastructure;
using Vertica.Integration.MongoDB.Infrastructure.Castle.Windsor;

namespace Vertica.Integration.MongoDB
{
    public class MongoDBConfiguration : IMongoDBConfiguration, IAdditionalConfiguration, IInitializable<IWindsorContainer>
    {
        private readonly List<IWindsorInstaller> _installers;

        internal MongoDBConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException("application");

            Application = application;
            Application.RegisterInitialization(this);

            _installers = new List<IWindsorInstaller>
            {
                new ConventionInstaller().AddFromAssemblyOfThis<MongoDBConfiguration>()
            };
        }

        public ApplicationConfiguration Application { get; private set; }

        public IAdditionalConfiguration Connection(ConnectionString connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");

            _installers.Add(new MongoDBInstaller(new DefaultConnection(connectionString)));

            return this;
        }

        public IAdditionalConfiguration AddConnection<TConnection>(TConnection connection)
            where TConnection : Connection
        {
            if (connection == null) throw new ArgumentNullException("connection");

            _installers.Add(new MongoDBInstaller<TConnection>(connection));

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
            foreach (IWindsorInstaller installer in _installers)
                container.Install(installer);
        }
    }
}