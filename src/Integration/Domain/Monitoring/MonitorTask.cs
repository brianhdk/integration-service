using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Email;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Domain.Monitoring
{
	public class MonitorTask : Task<MonitorWorkItem>
	{
		private readonly IConfigurationService _configuration;
		private readonly IEmailService _emailService;
		private readonly string[] _ignoreErrorsWithMessagesContaining;

        public MonitorTask(IEnumerable<IStep<MonitorWorkItem>> steps, IConfigurationService configuration, IEmailService emailService, string[] ignoreErrorsWithMessagesContaining)
			: base(steps)
		{
			_configuration = configuration;
			_emailService = emailService;

            _ignoreErrorsWithMessagesContaining = 
                (ignoreErrorsWithMessagesContaining ?? new string[0])
                    .Select(x => x.Replace("\\r\\n", Environment.NewLine))
                    .ToArray();
		}

		public override string Description
		{
			get { return "Monitors the solution and sends out e-mails if there is any errors."; }
		}

        public override MonitorWorkItem Start(ILog log, params string[] arguments)
		{
		    MonitorConfiguration configuration = _configuration.Get<MonitorConfiguration>();
		    configuration.Assert();

			string[] ignoredMessages =
				configuration.IgnoreErrorsWithMessagesContaining
                    .EmptyIfNull()
					.Concat(_ignoreErrorsWithMessagesContaining)
					.Where(x => !String.IsNullOrWhiteSpace(x))
					.Distinct()
					.ToArray();

			return new MonitorWorkItem(configuration.LastRun)
                .WithIgnoreFilter(new MessageContainsTextIgnoreFilter(ignoredMessages));
		}

        public override void End(MonitorWorkItem workItem, ILog log, params string[] arguments)
		{
            MonitorConfiguration configuration = _configuration.Get<MonitorConfiguration>();

		    foreach (MonitorTarget target in configuration.Targets)
		        SendTo(target, workItem, log, configuration.SubjectPrefix);

            configuration.LastRun = workItem.CheckRange.UpperBound;
		    _configuration.Save(configuration, "MonitorTask");
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

                // TODO: Group entries before sending

                log.Message("Sending {0} entries to {1}.", entries.Length, target);

	            var subject = new StringBuilder();

	            if (!String.IsNullOrWhiteSpace(subjectPrefix))
	                subject.AppendFormat("{0}: ", subjectPrefix);

	            subject.AppendFormat("Monitoring ({0})", workItem.CheckRange);

                _emailService.Send(new MonitorEmailTemplate(subject.ToString(), entries), target.Recipients);
	        }
	    }
	}
}