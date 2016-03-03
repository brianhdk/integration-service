using System;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure
{
	public class ProcessExitConfiguration : IInitializable<IWindsorContainer>
	{
		public ProcessExitConfiguration(AdvancedConfiguration advanced)
		{
			if (advanced == null) throw new ArgumentNullException(nameof(advanced));

			Advanced = advanced;
		}

		public AdvancedConfiguration Advanced { get; }

		void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			//
		}
	}
}