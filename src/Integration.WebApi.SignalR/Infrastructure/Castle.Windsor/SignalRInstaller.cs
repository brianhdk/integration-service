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
		private readonly IAssemblyLocator _assemblies;

	    public SignalRInstaller(Assembly[] assemblies)
	    {
		    _assemblies = new AssemblyLocator((assemblies ?? new Assembly[0]).Distinct().ToList());
	    }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
			container.RegisterInstance(_assemblies);
        }

		private class AssemblyLocator : IAssemblyLocator
        {
			private readonly IList<Assembly> _assemblies;

			public AssemblyLocator(IList<Assembly> assemblies)
			{
				_assemblies = assemblies;
			}

			public IList<Assembly> GetAssemblies()
			{
				return _assemblies;
			}
        }
	}
}