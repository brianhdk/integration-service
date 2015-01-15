using System;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Factories
{
	public static class ObjectFactory
	{
		private static readonly Func<IWindsorContainer> DefaultCreation = () => new WindsorContainer();

		private static Func<IWindsorContainer> _creation;
		public static IWindsorContainer Create(Func<IWindsorContainer> creation)
		{
			if (creation == null) throw new ArgumentNullException("creation");

			if (Container.IsValueCreated)
				throw new InvalidOperationException("Instance already accessed thereby created.");

			_creation = creation;

			return Instance;
		}

		private static readonly Lazy<IWindsorContainer> Container =
			new Lazy<IWindsorContainer>(() => (_creation ?? DefaultCreation)());

		public static IWindsorContainer Instance
		{
			get
			{
				return Container.Value;
			}
		}
	}
}