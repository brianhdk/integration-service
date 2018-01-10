using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Email;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;

namespace Vertica.Integration.Domain.Monitoring
{
    public class MonitorTask : IntegrationTask<MonitorWorkItem>
    {
	    private readonly IConfigurationService _configuration;
	    private readonly IEmailService _emailService;
	    private readonly IRuntimeSettings _runtimeSettings;

	    public MonitorTask(IEnumerable<IStep<MonitorWorkItem>> steps, IConfigurationService configuration, IEmailService emailService, IRuntimeSettings runtimeSettings)
			: base(steps)
		{
			_configuration = configuration;
			_emailService = emailService;
		    _runtimeSettings = runtimeSettings;
		}

		public override string Description => "Monitors the solution and sends out e-mails if there is any errors.";

	    public override MonitorWorkItem Start(ITaskExecutionContext context)
		{
		    MonitorConfiguration configuration = _configuration.Get<MonitorConfiguration>();
		    configuration.Assert();

			return new MonitorWorkItem(configuration)
                .AddIgnoreFilter(new MessageContainsText(configuration.IgnoreErrorsWithMessagesContaining))
                .AddTargetRedirect(new RedirectForMonitorTargets(configuration.Targets))
                .AddMessageGroupingPatterns(configuration.MessageGroupingPatterns);
		}

        public override void End(ITaskExecutionContext<MonitorWorkItem> context)
		{
            Target[] unconfiguredTargets;
            if (context.WorkItem.HasEntriesForUnconfiguredTargets(out unconfiguredTargets))
                context.Log.Error(Target.Service, "Create missing configuration for the following targets: [{0}].", string.Join(", ", unconfiguredTargets.Select(x => x.Name)));

		    foreach (MonitorTarget target in context.WorkItem.Configuration.Targets ?? new MonitorTarget[0])
		        SendTo(target, context);

            context.WorkItem.Configuration.LastRun = context.WorkItem.CheckRange.UpperBound;
		    _configuration.Save(context.WorkItem.Configuration, Name);
		}

        private void SendTo(MonitorTarget target, ITaskExecutionContext<MonitorWorkItem> context)
	    {
            MonitorEntry[] entries = context.WorkItem.GetEntries(target);

	        if (entries.Length > 0)
	        {
                if (target.Recipients == null || target.Recipients.Length == 0)
                {
                    context.Log.Warning(Target.Service, "No recipients found for target '{0}'.", target);
                    return;
                }

	            context.Log.Message("Sending {0} entries to {1}.", entries.Length, target);

	            var subject = new StringBuilder();

		        ApplicationEnvironment environment = _runtimeSettings.Environment;

		        if (environment != null)
			        subject.AppendFormat("[{0}] ", environment);

	            if (!string.IsNullOrWhiteSpace(context.WorkItem.Configuration.SubjectPrefix))
	                subject.AppendFormat("{0}: ", context.WorkItem.Configuration.SubjectPrefix);

	            subject.AppendFormat("Monitoring ({0})", context.WorkItem.CheckRange);

                _emailService.Send(new MonitorEmailTemplate(subject.ToString(), entries, target), target.Recipients);
	        }
	    }
	}
}