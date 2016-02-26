using System;

namespace Vertica.Integration.Tests
{
	public static class TestableApplicationContext
	{
		public static IApplicationContext Create(Action<ApplicationConfiguration> application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			var context = new ApplicationContext(configuration => configuration
				.Database(database => database.DisableIntegrationDb())
				.Logging(logging => logging.Disable())
				.RuntimeSettings(new InMemoryRuntimeSettings()
					.Set("Environment", "Testing"))
				.Change(application));

			return context;
		}
	}
}