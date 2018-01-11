using System.Collections.Generic;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Model;

namespace Vertica.Integration.Domain.Core
{
#pragma warning disable 618
	public class MaintenanceTask : Task<MaintenanceWorkItem>
#pragma warning restore 618
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