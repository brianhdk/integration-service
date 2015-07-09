using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Email;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;

namespace Vertica.Integration.Domain.Monitoring
{
    public class MonitorTask : Task<MonitorWorkItem>
	{
		private readonly IConfigurationService _configuration;
		private readonly IEmailService _emailService;

        public MonitorTask(IEnumerable<IStep<MonitorWorkItem>> steps, IConfigurationService configuration, IEmailService emailService)
			: base(steps)
		{
			_configuration = configuration;
			_emailService = emailService;
		}

		public override string Description
		{
			get { return "Monitors the solution and sends out e-mails if there is any errors."; }
		}

        public override MonitorWorkItem Start(ITaskExecutionContext context)
		{
		    MonitorConfiguration configuration = _configuration.Get<MonitorConfiguration>();
		    configuration.Assert();

			return new MonitorWorkItem(configuration)
                .AddIgnoreFilter(new MessageContainsText(configuration.IgnoreErrorsWithMessagesContaining))
                .AddTargetRedirect(new RedirectForMonitorTargets(configuration.Targets))
                .AddMessageGroupingPatterns(configuration.MessageGroupingPatterns);
		}

        public override void End(MonitorWorkItem workItem, ITaskExecutionContext context)
		{
            Target[] unconfiguredTargets;
            if (workItem.HasEntriesForUnconfiguredTargets(out unconfiguredTargets))
                context.Log.Error(Target.Service, "Create missing configuration for the following targets: [{0}].", String.Join(", ", unconfiguredTargets.Select(x => x.Name)));

		    foreach (MonitorTarget target in workItem.Configuration.Targets ?? new MonitorTarget[0])
		        SendTo(target, workItem, context.Log, workItem.Configuration.SubjectPrefix);

            workItem.Configuration.LastRun = workItem.CheckRange.UpperBound;
		    _configuration.Save(workItem.Configuration, "MonitorTask");
		}

        private void SendTo(MonitorTarget target, MonitorWorkItem workItem, ILog log, string subjectPrefix)
	    {
            MonitorEntry[] entries = workItem.GetEntries(target);

	        if (entries.Length > 0)
	        {
                if (target.Recipients == null || target.Recipients.Length == 0)
                {
                    log.Warning(Target.Service, "No recipients found for target '{0}'.", target);
                    return;
                }

                log.Message("Sending {0} entries to {1}.", entries.Length, target);

	            var subject = new StringBuilder();

	            if (!String.IsNullOrWhiteSpace(subjectPrefix))
	                subject.AppendFormat("{0}: ", subjectPrefix);

	            subject.AppendFormat("Monitoring ({0})", workItem.CheckRange);

                _emailService.Send(new MonitorEmailTemplate(subject.ToString(), entries, target), target.Recipients);
	        }
	    }
	}
}