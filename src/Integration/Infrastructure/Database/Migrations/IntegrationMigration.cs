using System;
using System.Collections.Specialized;
using System.Configuration;
using Castle.Windsor;
using FluentMigrator;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Factories;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    public abstract class IntegrationMigration : Migration
    {
        private readonly IWindsorContainer _factory;

        protected IntegrationMigration()
        {
            _factory = ObjectFactory.Instance;
        }

        protected T Resolve<T>()
        {
            return _factory.Resolve<T>();
        }

        protected IConfigurationService ConfigurationService
        {
            get { return Resolve<IConfigurationService>(); }
        }

        protected NameValueCollection AppSettings
        {
            get { return ConfigurationManager.AppSettings; }
        }

        protected Configuration.Configuration GetRawConfiguration(string id)
        {
            return ConfigurationService.Get(id);
        }

        protected T GetConfiguration<T>() where T : class, new()
        {
            return ConfigurationService.Get<T>();
        }

        protected void SaveRawConfiguration(Configuration.Configuration rawConfiguration)
        {
            if (rawConfiguration == null) throw new ArgumentNullException("rawConfiguration");

            ConfigurationService.Save(rawConfiguration, "Migration", createArchiveBackup: true);
        }

        protected void SaveConfiguration<T>(T configuration) where T : class, new()
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            ConfigurationService.Save(configuration, "Migration", createArchiveBackup: true);
        }

        protected void MergeConfiguration<T>(string id) where T : class, new()
        {
            Configuration.Configuration oldConfiguration = GetRawConfiguration(id);

            if (oldConfiguration != null)
            {
                // Ensure new configuration
                GetConfiguration<T>();

                // Get this new configuration as raw
                Configuration.Configuration newConfiguration = GetRawConfiguration(Configuration.ConfigurationService.GetGuidId<T>());

                // Copy json from the old configuration to the new one
                newConfiguration.JsonData = oldConfiguration.JsonData;

                // Save the new configuration
                SaveRawConfiguration(newConfiguration);

                // Delete the old configuration
                ConfigurationService.Delete(oldConfiguration.Id);
            }
        }

        protected void RunTask<TTask>() where TTask : ITask
        {
            ITask task = Resolve<ITaskFactory>().Get<TTask>();
            Resolve<ITaskRunner>().Execute(task);
        }
    }
}