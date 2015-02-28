using System;
using System.Collections.Generic;
using System.Globalization;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class ConfigurationTesterTask : Task<ConfigurationTesterWorkItem>
    {
        private readonly IConfigurationProvider _provider;

        public ConfigurationTesterTask(IEnumerable<IStep<ConfigurationTesterWorkItem>> steps, IConfigurationProvider provider)
            : base(steps)
        {
            _provider = provider;
        }

        public override ConfigurationTesterWorkItem Start(Log log, params string[] arguments)
        {
            // load up configuration
            // read it

            ConfigurationTesterConfiguration configuration = _provider.Get<ConfigurationTesterConfiguration>();

            return new ConfigurationTesterWorkItem(configuration);
        }

        public override void End(ConfigurationTesterWorkItem workItem, Log log, params string[] arguments)
        {
            // load up configuration, 
            //  - store it, back-up old version

            var configuration = _provider.Get<ConfigurationTesterConfiguration>();
            configuration.LastRun = DateTime.Now;

            _provider.Save(configuration, "ConfigurationTesterTask", createArchiveBackup: true);
        }

        public override string Description
        {
            get { return "TBD"; }
        }

        public override string Schedule
        {
            get { return "TBD"; }
        }
    }

    public class ConfigurationTesterConfiguration
    {
        public ConfigurationTesterConfiguration()
        {
            LastRun = DateTime.MinValue;
        }

        public DateTime LastRun { get; set; }
    }

    public class ConfigurationTesterStep : Step<ConfigurationTesterWorkItem>
    {
        public override string Description
        {
            get { return "TBD"; }
        }

        public override void Execute(ConfigurationTesterWorkItem workItem, Log log)
        {
            log.Message(workItem.Configuration.LastRun.ToString(CultureInfo.InvariantCulture));
        }
    }

    public class ConfigurationTesterWorkItem
    {
        public ConfigurationTesterConfiguration Configuration { get; set; }

        public ConfigurationTesterWorkItem(ConfigurationTesterConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}