using System;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.WindowsTaskScheduler
{
	internal class WindowsTaskSchedulerConfiguration : IInitializable<IWindsorContainer>
	{
		internal WindowsTaskSchedulerConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			Application = application
				.AddCustomInstaller(Install.ByConvention
					.AddFromAssemblyOfThis<WindowsTaskSchedulerConfiguration>());
		}

		public ApplicationConfiguration Application { get; }
        
		void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			container.RegisterInstance(this);
		}
	}
}