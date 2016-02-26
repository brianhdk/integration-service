using System;

namespace Vertica.Integration.Perfion
{
	public static class PerfionExtensions
	{
		public static ApplicationConfiguration UsePerfion(this ApplicationConfiguration application, Action<PerfionConfiguration> perfion = null)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			return application.Extensibility(extensibility =>
			{
				PerfionConfiguration configuration = extensibility.Register(() => new PerfionConfiguration(application));

				if (perfion != null)
					perfion(configuration);
			});
		}
	}
}