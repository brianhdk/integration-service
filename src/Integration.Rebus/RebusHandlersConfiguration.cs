using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Rebus.Handlers;

namespace Vertica.Integration.Rebus
{
	public class RebusHandlersConfiguration : IInitializable<IWindsorContainer>
	{
		private readonly List<Assembly> _scan;
		private readonly List<Type> _handlers;

		internal RebusHandlersConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			Application = application;

			_scan = new List<Assembly>();
            _handlers = new List<Type>();
		}

		public ApplicationConfiguration Application { get; }

		public RebusHandlersConfiguration AddFromAssemblyOfThis<T>()
		{
			_scan.Add(typeof(T).Assembly);

			return this;
		}

		public RebusHandlersConfiguration Handler<THandler>()
			where THandler : IHandleMessages
		{
			_handlers.Add(typeof(THandler));

			return this;
		}

		void IInitializable<IWindsorContainer>.Initialized(IWindsorContainer container)
		{
			foreach (Assembly assembly in _scan.Distinct())
			{
				container.Register(
					Classes.FromAssembly(assembly)
						.BasedOn(typeof(IHandleMessages<>))
						.Unless(_handlers.Contains)
						.WithServiceBase()
						.LifestyleTransient());
			}

			foreach (Type handler in _handlers.Distinct())
			{
				container.Register(
					Component.For(GetHandlerInterfaces(handler))
						.ImplementedBy(handler)
						.LifestyleTransient());
			}
		}

		private Type[] GetHandlerInterfaces(Type type)
		{
			return type.GetInterfaces()
				.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleMessages<>))
				.ToArray();
		}
	}
}