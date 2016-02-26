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
using Vertica.Integration.Model.Hosting;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration
{
	public class ApplicationConfiguration : IInitializable<IWindsorContainer>
    {
		private readonly ExtensibilityConfiguration _extensibility;

        private readonly List<IWindsorInstaller> _customInstallers;

        private readonly DatabaseConfiguration _database;
        private readonly TasksConfiguration _tasks;
        private readonly LoggingConfiguration _logging;
        private readonly MigrationConfiguration _migration;
        private readonly HostsConfiguration _hosts;
		private readonly AdvancedConfiguration _advanced;

		internal ApplicationConfiguration()
        {
			_extensibility = new ExtensibilityConfiguration();

            IgnoreSslErrors = true;

            _customInstallers = new List<IWindsorInstaller>();

            _database = Register(() => new DatabaseConfiguration(this));
            _tasks = Register(() => new TasksConfiguration(this));
            _logging = Register(() => new LoggingConfiguration(this));
            _migration = Register(() => new MigrationConfiguration(this));
            _hosts = Register(() => new HostsConfiguration(this));
			_advanced = Register(() => new AdvancedConfiguration(this));

			Register(() => this);
        }

		private T Register<T>(Func<T> factory) where T : class
		{
			T result = null;

			Extensibility(extensibility => result = extensibility.Register(factory));

			return result;
		}

        public bool IgnoreSslErrors { get; set; }

        public ApplicationConfiguration AddCustomInstaller(IWindsorInstaller installer)
        {
            return AddCustomInstallers(installer);
        }

        public ApplicationConfiguration AddCustomInstallers(params IWindsorInstaller[] installers)
        {
            if (installers != null)
                _customInstallers.AddRange(installers.SkipNulls());

            return this;
        }

        public ApplicationConfiguration RegisterDependency<T>(T singletonInstance) where T : class
        {
            if (singletonInstance == null) throw new ArgumentNullException(nameof(singletonInstance));

            return AddCustomInstaller(new InstanceInstaller<T>(singletonInstance));
        }

        public ApplicationConfiguration Tasks(Action<TasksConfiguration> tasks)
        {
            if (tasks != null)
                tasks(_tasks);

            return this;
        }

        public ApplicationConfiguration Logging(Action<LoggingConfiguration> logging)
        {
            if (logging != null)
                logging(_logging);

            return this;
        }

        public ApplicationConfiguration Database(Action<DatabaseConfiguration> database)
        {
            if (database != null)
                database(_database);

            return this;
        }

        public ApplicationConfiguration Migration(Action<MigrationConfiguration> migration)
        {
            if (migration != null)
                migration(_migration);

            return this;
        }

        public ApplicationConfiguration Hosts(Action<HostsConfiguration> hosts)
        {
            if (hosts != null)
                hosts(_hosts);

            return this;
        }

		public ApplicationConfiguration Advanced(Action<AdvancedConfiguration> advanced)
		{
			if (advanced != null)
				advanced(_advanced);

			return this;
		}

		public ApplicationConfiguration Extensibility(Action<ExtensibilityConfiguration> extensibility)
		{
			if (extensibility != null)
				extensibility(_extensibility);

			return this;
		}

		public ApplicationConfiguration RuntimeSettings<T>()
			where T : IRuntimeSettings
		{
			return Advanced(advanced => advanced.Register<IRuntimeSettings, T>());
		}

		public ApplicationConfiguration RuntimeSettings<T>(T instance)
			where T : IRuntimeSettings
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));

			return Advanced(advanced => advanced.Register<IRuntimeSettings>(() => instance));
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
    }
}