using System;

namespace Vertica.Integration.Globase
{
	public static class GlobaseExtensions
	{
		public static ApplicationConfiguration UseGlobase(this ApplicationConfiguration application, Action<GlobaseConfiguration> globase = null)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			return application.Extensibility(extensibility =>
			{
				GlobaseConfiguration configuration = extensibility.Register(() => new GlobaseConfiguration(application));

				globase?.Invoke(configuration);
			});
		}
	}
}