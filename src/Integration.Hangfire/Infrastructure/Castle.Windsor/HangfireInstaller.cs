using System;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Hangfire.Server;

namespace Vertica.Integration.Hangfire.Infrastructure.Castle.Windsor
{
	internal class HangfireInstaller : IWindsorInstaller
	{
		private readonly Assembly[] _scanAssemblies;
        private readonly Type[] _addProcesses;
        private readonly Type[] _removeProcesses;

	    public HangfireInstaller(Assembly[] scanAssemblies, Type[] addProcesses, Type[] removeProcesses)
        {
		    if (scanAssemblies == null) throw new ArgumentNullException(nameof(scanAssemblies));
		    if (addProcesses == null) throw new ArgumentNullException(nameof(addProcesses));
		    if (removeProcesses == null) throw new ArgumentNullException(nameof(removeProcesses));

		    _scanAssemblies = scanAssemblies;
		    _addProcesses = addProcesses;
		    _removeProcesses = removeProcesses;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            foreach (Assembly assembly in _scanAssemblies.Distinct())
            {
                container.Register(
                    Classes.FromAssembly(assembly)
                        .BasedOn<IBackgroundProcess>()
						.WithServiceFromInterface(typeof(IBackgroundProcess))
                        .Unless(x =>
                        {
                            if (_removeProcesses.Contains(x))
                                return true;

                            return false;
                        }));
            }

            container.Register(
                Classes.From(_addProcesses.Except(_removeProcesses).Distinct())
                    .BasedOn<IBackgroundProcess>()
					.WithServiceFromInterface(typeof(IBackgroundProcess)));
        }
	}
}