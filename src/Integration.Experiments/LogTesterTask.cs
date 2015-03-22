using System;
using System.Collections.Generic;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class LogTesterTask : Task<LogTesterWorkItem>
    {
        private readonly ILogger _logger;

        public LogTesterTask(IEnumerable<IStep<LogTesterWorkItem>> steps, ILogger logger)
            : base(steps)
        {
            _logger = logger;
        }

        public override LogTesterWorkItem Start(Log log, params string[] arguments)
        {
            log.Message("task");

            return new LogTesterWorkItem();
        }

        public override void End(LogTesterWorkItem workItem, Log log, params string[] arguments)
        {
            log.Warning(Target.Service, "warning");
            _logger.LogError(new ApplicationException(), Target.Service);
        }

        public override string Description
        {
            get { return "TBD"; }
        }
    }

    public class LogTesterStep : Step<LogTesterWorkItem>
    {
        public override string Description
        {
            get { return "TBD"; }
        }

        public override void Execute(LogTesterWorkItem workItem, Log log)
        {
            log.Message("step");
            log.Error(Target.Service, "Test message");
        }
    }

    public class LogTesterWorkItem
    {
    }
}