using System;
using System.Collections.Specialized;
using System.Configuration;
using Castle.MicroKernel;
using FluentMigrator;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Model;
using Arguments = Vertica.Integration.Model.Arguments;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	public abstract class IntegrationMigration : Migration
	{
		public T Resolve<T>()
		{
			return Kernel.Resolve<T>();
		}

		protected IConfigurationService ConfigurationService => Resolve<IConfigurationService>();

		protected IConfigurationRepository ConfigurationRepository => Resolve<IConfigurationRepository>();

		protected IRuntimeSettings RuntimeSettings => Resolve<IRuntimeSettings>();

		protected NameValueCollection AppSettings => ConfigurationManager.AppSettings;

		protected Configuration.Configuration GetRawConfiguration(string id)
		{
			return ConfigurationRepository.Get(id);
		}

		protected T GetConfiguration<T>() where T : class, new()
		{
			return ConfigurationService.Get<T>();
		}

		protected void SaveRawConfiguration(Configuration.Configuration rawConfiguration)
		{
			if (rawConfiguration == null) throw new ArgumentNullException(nameof(rawConfiguration));

			ConfigurationService.Save(rawConfiguration, "Migration", createArchiveBackup: true);
		}

		protected void SaveConfiguration<T>(T configuration) where T : class, new()
		{
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));

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
				ConfigurationRepository.Delete(oldConfiguration.Id);
			}
		}

		protected ConfigurationUpdater<T> UpdateConfiguration<T>() where T : class, new()
		{
			return new ConfigurationUpdater<T>(GetConfiguration<T>(), SaveConfiguration);
		}

		protected void RunTask(string name, Arguments arguments = null)
		{
			RunTask(GetTask(name), arguments);
		}

		protected void RunTask<TTask>(Arguments arguments = null) where TTask : class, ITask
		{
			RunTask(GetTask<TTask>(), arguments);
		}

		protected void RunTask(ITask task, Arguments arguments = null)
		{
			Resolve<ITaskRunner>().Execute(task, arguments ?? new Arguments());
		}

		protected ITask GetTask(string name)
		{
			return Resolve<ITaskFactory>().Get(name);
		}

		protected ITask GetTask<TTask>() where TTask : class, ITask
		{
			return Resolve<ITaskFactory>().Get<TTask>();
		}
		
		public override void Down()
		{
		}

		private IKernel Kernel
		{
		    get
		    {
		        var kernel = ApplicationContext as IKernel;

                if (kernel == null)
                    throw new InvalidOperationException($"Unable to cast {nameof(ApplicationContext)} to {nameof(IKernel)}. ApplicationContext: {ApplicationContext?.ToString() ?? "<null>"}");

                return kernel;
            }
        }
	}
}