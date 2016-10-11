using System;
using Castle.Windsor;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Slack
{
	public class SlackConfiguration : IInitializable<IWindsorContainer>
	{
		internal SlackConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			Application = application
				.AddCustomInstaller(Install.ByConvention
					.AddFromAssemblyOfThis<SlackConfiguration>()
					.Ignore<SlackConfiguration>());
		}

		public SlackConfiguration Change(Action<SlackConfiguration> change)
		{
			change?.Invoke(this);

			return this;
		}

		public ApplicationConfiguration Application { get; }

		/// <summary>
		/// Adds Slack to <see cref="ILiteServerFactory"/> allowing Slack to run simultaneously with other servers.
		/// </summary>
		public SlackConfiguration AddToLiteServer()
		{
			Application.UseLiteServer(server => server.AddServer<SlackBackgroundServer>());

			return this;
		}

		void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			container.RegisterInstance(this, x => x.LifestyleSingleton());
		}
	}
}