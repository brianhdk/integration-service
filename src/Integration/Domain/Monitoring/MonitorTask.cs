using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Email;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Integration.Properties;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Domain.Monitoring
{
	public class MonitorTask : Task<MonitorWorkItem>
	{
		private readonly IParametersProvider _parametersProvider;
		private readonly ISettings _settings;
		private readonly IEmailService _emailService;
		private readonly string[] _ignoreErrorsWithMessagesContaining;

		public MonitorTask(IEnumerable<IStep<MonitorWorkItem>> steps, IParametersProvider parametersProvider, ISettings settings, IEmailService emailService, string[] ignoreErrorsWithMessagesContaining)
			: base(steps)
		{
			_parametersProvider = parametersProvider;
			_settings = settings;
			_emailService = emailService;
			_ignoreErrorsWithMessagesContaining = ignoreErrorsWithMessagesContaining ?? new string[0];
		}

		public override string Description
		{
			get { return "Monitors the solution and sends out e-mails if there is any errors."; }
		}

		public override string Schedule
		{
			get { return "Every 15 minutes between 05:00 and 23:00."; }
		}

		public override MonitorWorkItem Start(Log log, params string[] arguments)
		{
			string[] ignoredMessages =
				(_settings.IgnoreErrorsWithMessagesContaining ?? new StringCollection())
					.OfType<string>()
					.Concat(_ignoreErrorsWithMessagesContaining)
					.SkipNulls()
					.Distinct()
					.Select(x => x.Replace("\\r\\n", Environment.NewLine))
					.ToArray();

			Parameters parameters = _parametersProvider.Get();

			return new MonitorWorkItem(parameters.LastMonitorCheck, new MessageContainsTextIgnoreFilter(ignoredMessages));
		}

		public override void End(MonitorWorkItem workItem, Log log, params string[] arguments)
		{
		    SendTo(Target.Service, workItem, log, _settings.MonitorEmailRecipientsForService);
            SendTo(Target.Business, workItem, log, _settings.MonitorEmailRecipientsForBusiness);

		    Parameters parameters = _parametersProvider.Get();
			parameters.LastMonitorCheck = workItem.CheckRange.UpperBound;

			_parametersProvider.Save(parameters);
		}

	    private void SendTo(Target target, MonitorWorkItem workItem, Log log, StringCollection recipients)
	    {
	        if (recipients == null)
	        {
	            log.Warning(Target.Service, "No recipients found for target '{0}'.", target);
	            return;
	        }

            MonitorEntry[] entries = workItem.GetEntries(target);

	        if (entries.Length > 0)
	        {
                log.Message("Sending {0} entries to {1}.", entries.Length, target);

	            _emailService.Send(new MonitorEmailTemplate(workItem.CheckRange, entries), recipients.Cast<string>());
	        }
	    }
	}
}