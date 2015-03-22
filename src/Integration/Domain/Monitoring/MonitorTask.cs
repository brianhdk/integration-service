using System;
using System.Collections.Generic;
using System.Linq;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Email;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Domain.Monitoring
{
	public class MonitorTask : Task<MonitorWorkItem>
	{
		private readonly IConfigurationProvider _configuration;
		private readonly IEmailService _emailService;
		private readonly string[] _ignoreErrorsWithMessagesContaining;

        public MonitorTask(IEnumerable<IStep<MonitorWorkItem>> steps, IConfigurationProvider configuration, IEmailService emailService, string[] ignoreErrorsWithMessagesContaining)
			: base(steps)
		{
			_configuration = configuration;
			_emailService = emailService;
			_ignoreErrorsWithMessagesContaining = ignoreErrorsWithMessagesContaining ?? new string[0];
		}

		public override string Description
		{
			get { return "Monitors the solution and sends out e-mails if there is any errors."; }
		}

		public override MonitorWorkItem Start(Log log, params string[] arguments)
		{
		    MonitorConfiguration configuration = _configuration.Get<MonitorConfiguration>();
		    configuration.Assert();

			string[] ignoredMessages =
				configuration.IgnoreErrorsWithMessagesContaining
                    .EmptyIfNull()
					.Concat(_ignoreErrorsWithMessagesContaining)
					.SkipNulls()
					.Distinct()
					.Select(x => x.Replace("\\r\\n", Environment.NewLine))
					.ToArray();

		    bool updateLastRun;

		    DateTimeOffset lastRun;
            if (DateTimeOffset.TryParse(arguments.FirstOrDefault(), out lastRun))
            {
                log.Message("Monitor starting from {0}", lastRun);
                updateLastRun = false;
            }
            else
            {
                lastRun = configuration.LastRun;
                updateLastRun = true;
            }

			return new MonitorWorkItem(lastRun, updateLastRun)
                .WithIgnoreFilter(new MessageContainsTextIgnoreFilter(ignoredMessages));
		}

		public override void End(MonitorWorkItem workItem, Log log, params string[] arguments)
		{
            MonitorConfiguration configuration = _configuration.Get<MonitorConfiguration>();

		    foreach (MonitorTarget target in configuration.Targets)
		        SendTo(target, workItem, log, target.Recipients);

		    if (workItem.UpdateLastCheck)
		    {
                configuration.LastRun = workItem.CheckRange.UpperBound;
		        _configuration.Save(configuration, "MonitorTask");
		    }
		}

	    private void SendTo(Target target, MonitorWorkItem workItem, Log log, string[] recipients)
	    {
            MonitorEntry[] entries = workItem.GetEntries(target);

	        if (entries.Length > 0)
	        {
                if (recipients == null)
                {
                    log.Warning(Target.Service, "No recipients found for target '{0}'.", target);
                    return;
                }

                // TODO: Group entries before sending

                log.Message("Sending {0} entries to {1}.", entries.Length, target);

                _emailService.Send(new MonitorEmailTemplate(workItem.CheckRange, entries), recipients);
	        }
	    }
	}
}