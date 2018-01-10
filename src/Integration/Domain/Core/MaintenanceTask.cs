using System.Collections.Generic;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Model;

namespace Vertica.Integration.Domain.Core
{
	public class MaintenanceTask : IntegrationTask<MaintenanceWorkItem>
	{
	    private readonly IConfigurationService _service;

		public MaintenanceTask(IEnumerable<IStep<MaintenanceWorkItem>> steps, IConfigurationService service) 
			: base(steps)
		{
		    _service = service;
		}

	    public override string Description => "Performs a number of steps to clean up the solution.";

		public override MaintenanceWorkItem Start(ITaskExecutionContext context)
		{
			return new MaintenanceWorkItem(_service.Get<MaintenanceConfiguration>());
		}
	}
}