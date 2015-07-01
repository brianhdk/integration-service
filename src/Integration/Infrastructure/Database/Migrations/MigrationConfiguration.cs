using System;
using System.Collections.Generic;
using Castle.Windsor;
using FluentMigrator;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    public class MigrationConfiguration : IInitializable<IWindsorContainer>
    {
        private readonly List<MigrationTarget> _customTargets;

        internal MigrationConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException("application");

            Application = application;

            _customTargets = new List<MigrationTarget>();

            ChangeIntegrationDbDatabaseServer(DatabaseServer.SqlServer2014);
            CheckExistsIntegrationDb = true;
        }

        public ApplicationConfiguration Application { get; private set; }

        public MigrationConfiguration ChangeIntegrationDbDatabaseServer(DatabaseServer db)
        {
            IntegrationDbDatabaseServer = db;

            return this;
        }

        /// <summary>
        /// Makes it possible to execute custom migrations where the VersionInfo table will be stored in the IntegrationDb.
        /// Tip: Inherit from <see cref="IntegrationMigration"/> to have access to all services from the Integration Service.
        /// </summary>
        public MigrationConfiguration AddFromNamespaceOfThis<T>()
            where T : Migration
        {
            return AddFromNamespaceOfThis<T>(IntegrationDbDatabaseServer, Application.DatabaseConnectionString);
        }

        /// <summary>
        /// Makes it possible to execute custom migrations against any database.
        /// </summary>
        /// <param name="db">Specifies the version of the database where the migrations are executed against.</param>
        /// <param name="connectionString">Specifies the ConnectionString to the database.</param>
        public MigrationConfiguration AddFromNamespaceOfThis<T>(DatabaseServer db, ConnectionString connectionString)
            where T : Migration
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

        internal bool IntegrationDbDisabled
        {
            get
            {
                bool result = false;

                Application.Database(database => result = database.IntegrationDbDisabled);

                return result;
            }
        }

        internal DatabaseServer IntegrationDbDatabaseServer { get; private set; }
        internal bool CheckExistsIntegrationDb { get; private set; }

        internal MigrationTarget[] CustomTargets
        {
            get { return _customTargets.ToArray(); }
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
            container.RegisterInstance(this);
        }
    }
}