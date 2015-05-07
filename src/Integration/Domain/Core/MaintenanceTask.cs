using System.Collections.Generic;
using Vertica.Integration.Model;

namespace Vertica.Integration.Domain.Core
{
	public class MaintenanceTask : Task<MaintenanceWorkItem>
	{
		public MaintenanceTask(IEnumerable<IStep<MaintenanceWorkItem>> steps) 
			: base(steps)
		{
		}

		public override string Description
		{
			get { return "Performs a number of steps to clean up the solution."; }
		}

        public override MaintenanceWorkItem Start(ILog log, params string[] arguments)
		{
			return new MaintenanceWorkItem();
		}
	}
}