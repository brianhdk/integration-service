using System;
using System.Collections.Generic;
using System.Globalization;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class ConfigurationTesterTask : Task<ConfigurationTesterWorkItem>
    {
        private readonly IConfigurationService _service;

        public ConfigurationTesterTask(IEnumerable<IStep<ConfigurationTesterWorkItem>> steps, IConfigurationService service)
            : base(steps)
        {
            _service = service;
        }

        public override ConfigurationTesterWorkItem Start(Log log, params string[] arguments)
        {
            ConfigurationTesterConfiguration configuration = _service.Get<ConfigurationTesterConfiguration>();

            return new ConfigurationTesterWorkItem(configuration);
        }

        public override void End(ConfigurationTesterWorkItem workItem, Log log, params string[] arguments)
        {
            var configuration = _service.Get<ConfigurationTesterConfiguration>();

            configuration.LastRun = DateTime.Now;

            _service.Save(configuration, "ConfigurationTesterTask", createArchiveBackup: true);
        }

        public override string Description
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