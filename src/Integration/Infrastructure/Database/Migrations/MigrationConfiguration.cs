using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    public class MigrationConfiguration
    {
        private readonly List<MigrationTarget> _customTargets;

        internal MigrationConfiguration()
        {
            _customTargets = new List<MigrationTarget>();

            ChangeIntegrationDbDatabaseServer(DatabaseServer.SqlServer2014);
            CheckExistsIntegrationDb = true;
        }

        public MigrationConfiguration ChangeIntegrationDbDatabaseServer(DatabaseServer db)
        {
            IntegrationDbDatabaseServer = db;

            return this;
        }

        public MigrationConfiguration IncludeFromNamespaceOfThis<T>(DatabaseServer db, ConnectionString connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");

            _customTargets.Add(new MigrationTarget(
                db,
                connectionString,
                typeof(T).Assembly,
                typeof(T).Namespace));

            return this;
        }

        public MigrationConfiguration DisableCheckExistsIntegrationDb()
        {
            CheckExistsIntegrationDb = false;

            return this;
        }

        internal DatabaseServer IntegrationDbDatabaseServer { get; private set; }
        internal bool CheckExistsIntegrationDb { get; private set; }

        internal MigrationTarget[] CustomTargets
        {
            get { return _customTargets.ToArray(); }
        }

        internal void Install(IWindsorContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");

            container.Register(
                Component.For<MigrationConfiguration>()
                    .UsingFactoryMethod(() => this));
        }
    }
}