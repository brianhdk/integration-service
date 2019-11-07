using System.Collections.Generic;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Tasks;

namespace Vertica.Integration.Domain.Core
{
    [PreventConcurrentTaskExecution]
#pragma warning disable 618
	public class MaintenanceTask : Task<MaintenanceWorkItem>
#pragma warning restore 618
	{
	    private const string ConfigurationName = "74232608-79AD-4615-8ED7-2074301DFE56";

	    private readonly IConfigurationService _configuration;

		public MaintenanceTask(IEnumerable<IStep<MaintenanceWorkItem>> steps, IConfigurationService configuration) 
			: base(steps)
		{
		    _configuration = configuration;
		}

	    public override string Description => "Performs a number of steps to clean up the solution.";

	    public override bool IsDisabled(ITaskExecutionContext context)
	    {   
	        MaintenanceConfiguration configuration = _configuration.Get<MaintenanceConfiguration>();

	        context.TypedBag(ConfigurationName, configuration);

	        return configuration.Disabled;
	    }

		public override MaintenanceWorkItem Start(ITaskExecutionContext context)
		{
		    MaintenanceConfiguration configuration = context.TypedBag<MaintenanceConfiguration>(ConfigurationName);

			return new MaintenanceWorkItem(configuration);
		}
	}
}