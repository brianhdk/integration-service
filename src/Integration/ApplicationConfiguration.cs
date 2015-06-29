using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Web;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration
{
    public class ApplicationConfiguration : IInitializable<IWindsorContainer>
    {
        private readonly List<IWindsorInstaller> _customInstallers;
        private readonly DatabaseConfiguration _database;
        private readonly TasksConfiguration _tasks;
        private readonly LoggingConfiguration _logging;
        private readonly WebApiConfiguration _webApi;
        private readonly MigrationConfiguration _migration;

        internal ApplicationConfiguration()
        {
            IgnoreSslErrors = true;

            _customInstallers = new List<IWindsorInstaller>();
            _database = new DatabaseConfiguration(this);
            _tasks = new TasksConfiguration(this);
            _logging = new LoggingConfiguration(this);
            _webApi = new WebApiConfiguration(this);
            _migration = new MigrationConfiguration(this);
        }

        public bool IgnoreSslErrors { get; set; }
        public ConnectionString DatabaseConnectionString { get { return _database.ConnectionString; } }

        public ApplicationConfiguration AddCustomInstaller(IWindsorInstaller installer)
        {
            if (installer != null)
                AddCustomInstallers(installer);

            return this;
        }

        public ApplicationConfiguration AddCustomInstallers(params IWindsorInstaller[] installers)
        {
            if (installers != null)
                _customInstallers.AddRange(installers.SkipNulls());

            return this;
        }

        public ApplicationConfiguration RegisterDependency<T>(T singletonInstance) where T : class
        {
            if (singletonInstance == null) throw new ArgumentNullException("singletonInstance");

            return AddCustomInstaller(new InstanceInstaller<T>(singletonInstance));
        }

        public ApplicationConfiguration Tasks(Action<TasksConfiguration> tasks)
        {
            if (tasks != null)
                tasks(_tasks);

            return this;
        }

        public ApplicationConfiguration Logging(Action<LoggingConfiguration> logger)
        {
            if (logger != null)
                logger(_logging);

            return this;
        }

        public ApplicationConfiguration Database(Action<DatabaseConfiguration> database)
        {
            if (database != null)
                database(_database);

            return this;
        }

        public ApplicationConfiguration WebApi(Action<WebApiConfiguration> webApi)
        {
            if (webApi != null)
                webApi(_webApi);

            return this;
        }

        public ApplicationConfiguration Migration(Action<MigrationConfiguration> migration)
        {
            if (migration != null)
                migration(_migration);

            return this;
        }

        public ApplicationConfiguration Change(Action<ApplicationConfiguration> change)
        {
            if (change != null)
                change(this);

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
            container.Install(_customInstallers.ToArray());
        }

        internal IEnumerable<IInitializable<IWindsorContainer>> ContainerInitializations
        {
            get
            {
                yield return _database;
                yield return _tasks;
                yield return _logging;
                yield return _webApi;
                yield return _migration;
                yield return this;
            }
        }
    }
}