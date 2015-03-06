using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Web;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration
{
    public class ApplicationConfiguration
    {
        private readonly List<IWindsorInstaller> _customInstallers;
        private readonly AutoRegistredTasksConfiguration _autoRegistredTasks;
        private readonly WebApiConfiguration _webApi;
        private readonly MigrationConfiguration _migration;

        internal ApplicationConfiguration()
        {
            _customInstallers = new List<IWindsorInstaller>();
            _autoRegistredTasks = new AutoRegistredTasksConfiguration();
            _webApi = new WebApiConfiguration();
            _migration = new MigrationConfiguration();

            DatabaseConnectionStringName = "IntegrationDb";
            IgnoreSslErrors = true;
        }

        public ApplicationConfiguration AddCustomInstaller(IWindsorInstaller installer)
        {
            if (installer == null) throw new ArgumentNullException("installer");

            AddCustomInstallers(installer);

            return this;
        }

        public ApplicationConfiguration AddCustomInstallers(params IWindsorInstaller[] installers)
        {
            _customInstallers.AddRange(installers.EmptyIfNull().SkipNulls());

            return this;
        }

        public IWindsorInstaller[] CustomInstallers
        {
            get { return _customInstallers.ToArray(); }
        }

        public ApplicationConfiguration AutoRegistredTasks(Action<AutoRegistredTasksConfiguration> autoRegistredTasks)
        {
            if (autoRegistredTasks == null) throw new ArgumentNullException("autoRegistredTasks");

            autoRegistredTasks(_autoRegistredTasks);

            return this;
        }

        public ApplicationConfiguration WebApi(Action<WebApiConfiguration> webApi)
        {
            if (webApi == null) throw new ArgumentNullException("webApi");

            webApi(_webApi);

            return this;
        }

        public ApplicationConfiguration Migration(Action<MigrationConfiguration> migration)
        {
            if (migration == null) throw new ArgumentNullException("migration");

            migration(_migration);

            return this;
        }

        public ApplicationConfiguration Change(Action<ApplicationConfiguration> change)
        {
            if (change == null) throw new ArgumentNullException("change");

            change(this);

            return this;
        }

        public string DatabaseConnectionStringName { get; set; }
        public bool IgnoreSslErrors { get; set; }

        public ApplicationConfiguration Nothing()
        {
            return this;
        }
    }
}