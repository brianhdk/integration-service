using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Model;

namespace Experiments.Files
{
	public class StartupActions : KernelActions<StartupActions>
	{
		public StartupActions RunMigrateTask()
		{
			Add(kernel =>
			{
				var factory = kernel.Resolve<ITaskFactory>();
				var runner = kernel.Resolve<ITaskRunner>();

				runner.Execute(factory.Get<MigrateTask>());
			});

			return this;
		}

		protected override StartupActions This => this;
	}
}