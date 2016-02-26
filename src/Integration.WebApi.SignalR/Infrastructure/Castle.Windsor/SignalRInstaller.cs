using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Microsoft.AspNet.SignalR.Hubs;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.WebApi.SignalR.Infrastructure.Castle.Windsor
{
	internal class SignalRInstaller : IWindsorInstaller
	{
		private readonly Assembly[] _assemblies;
		private readonly Type[] _removeHubs;
		private readonly Type[] _removePipelines;

		public SignalRInstaller(Assembly[] assemblies, Type[] removeHubs, Type[] removePipelines)
	    {
		    _assemblies = (assemblies ?? new Assembly[0]).Distinct().ToArray();
		    _removeHubs = (removeHubs ?? new Type[0]).Distinct().ToArray();
			_removePipelines = (removePipelines ?? new Type[0]).Distinct().ToArray();
	    }

		public void Install(IWindsorContainer container, IConfigurationStore store)
        {
			var hubs = new List<Type>();

			foreach (Assembly assembly in _assemblies)
			{
				container.Register(
					Classes.FromAssembly(assembly)
						.BasedOn<IHub>()
						.Unless(x =>
						{
							if (_removeHubs.Contains(x))
								return true;

							hubs.Add(x);
							return false;
						})
						.WithServiceSelf()
						.LifestyleTransient());

				container.Register(
					Classes.FromAssembly(assembly)
						.BasedOn<IHubPipelineModule>()
						.Unless(_removePipelines.Contains)
						.WithService.FromInterface());

				// TODO: Lifestyle?
			}

			container.RegisterInstance<IHubsProvider>(new SignalRHubs(hubs.ToArray()));
        }

		private class SignalRHubs : IHubsProvider
		{
			private readonly Type[] _hubs;

			public SignalRHubs(Type[] hubs)
			{
				_hubs = hubs;
			}

			public Type[] Hubs => _hubs;
		}
	}
}