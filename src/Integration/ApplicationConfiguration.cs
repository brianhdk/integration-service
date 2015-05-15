using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database.Dapper;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Web;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration
{
    public class ApplicationConfiguration
    {
        private readonly List<IWindsorInstaller> _customInstallers;
        private readonly TasksConfiguration _tasks;
        private readonly DapperConfiguration _dapper;
        private readonly WebApiConfiguration _webApi;
        private readonly MigrationConfiguration _migration;

        internal ApplicationConfiguration()
        {
            DatabaseConnectionString = ConnectionString.FromName("IntegrationDb");
            IgnoreSslErrors = true;

            _customInstallers = new List<IWindsorInstaller>();
            _tasks = new TasksConfiguration();
            _dapper = new DapperConfiguration(this);
            _webApi = new WebApiConfiguration();
            _migration = new MigrationConfiguration(this);

        }

        public ConnectionString DatabaseConnectionString { get; set; }
        public bool IgnoreSslErrors { get; set; }

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

        internal IWindsorInstaller[] CustomInstallers
        {
            get { return _customInstallers.ToArray(); }
        }

        public ApplicationConfiguration Tasks(Action<TasksConfiguration> tasks)
        {
            if (tasks != null)
                tasks(_tasks);

            return this;
        }

        public ApplicationConfiguration Dapper(Action<DapperConfiguration> dapper)
        {
            if (dapper != null)
                dapper(_dapper);

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
    }
}