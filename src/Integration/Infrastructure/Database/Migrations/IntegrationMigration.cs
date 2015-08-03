using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using Castle.MicroKernel;
using FluentMigrator;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting;
using Vertica.Integration.Model.Hosting.Handlers;
using Arguments = Vertica.Integration.Model.Arguments;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    public abstract class IntegrationMigration : Migration
    {
        protected T Resolve<T>()
        {
            return Kernel.Resolve<T>();
        }

        protected IConfigurationService ConfigurationService
        {
            get { return Resolve<IConfigurationService>(); }
        }

	    protected IRuntimeSettings RuntimeSettings
	    {
			get { return Resolve<IRuntimeSettings>(); }
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

        protected ConfigurationUpdater<T> UpdateConfiguration<T>() where T : class, new()
        {
            return new ConfigurationUpdater<T>(GetConfiguration<T>(), SaveConfiguration);
        }

        protected void RunTask<TTask>() where TTask : class, ITask
        {
            ITask task = GetTask<TTask>();

            Resolve<ITaskRunner>().Execute(task);
        }

	    protected ITask GetTask<TTask>() where TTask : class, ITask
	    {
		    return Resolve<ITaskFactory>().Get<TTask>();
	    }

		protected void InstallAsWindowsService<TTask>(Action<InstallWindowsService> serviceArgs = null, Arguments arguments = null) where TTask : class, ITask
	    {
			ITask task = GetTask<TTask>();

			InstallAsWindowsService(task.Name(), task.Description, serviceArgs, arguments);
	    }

	    public void InstallAsWindowsService(string command, string description, Action<InstallWindowsService> serviceArgs = null, Arguments arguments = null)
	    {
		    if (String.IsNullOrWhiteSpace(command)) throw new ArgumentException(@"Value cannot be null or empty.", "command");
		    if (String.IsNullOrWhiteSpace(description)) throw new ArgumentException(@"Value cannot be null or empty.", "description");

		    var handler = Resolve<IWindowsServiceHandler>();

		    var windowsService = new WindowsService(command, description);

			var install = new InstallWindowsService();

			if (serviceArgs != null)
				serviceArgs(install);

		    KeyValuePair<string, string>[] args = (arguments ?? new Arguments()).ToArray();

		    handler.Handle(new HostArguments(command, install.ToArray(), args), windowsService);
	    }

	    protected void UninstallWindowsService<TTask>() where TTask : class, ITask
		{
			UninstallWindowsService(typeof(TTask).TaskName());
		}

	    public void UninstallWindowsService(string command)
	    {
		    if (String.IsNullOrWhiteSpace(command)) throw new ArgumentException(@"Value cannot be null or empty.", "command");

		    var handler = Resolve<IWindowsServiceHandler>();

		    KeyValuePair<string, string>[] commandArgs = { WindowsServiceHandler.UninstallCommand };

		    handler.Handle(
			    new HostArguments(command, commandArgs, Arguments.Empty.ToArray()),
			    new WindowsService(command, "dummy"));
	    }

	    public override void Down()
	    {
	    }

	    private IKernel Kernel
        {
            get { return ApplicationContext as IKernel; }
        }
    }
}