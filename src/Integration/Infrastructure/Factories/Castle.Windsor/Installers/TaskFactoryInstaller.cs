using System;
using System.Linq;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Exceptions;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
    internal class TaskFactoryInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
		    container.Register(
		        Component.For<ITaskFactory>()
		            .UsingFactoryMethod(kernel => new TaskFactory(kernel)));
		}

        private class TaskFactory : ITaskFactory
        {
            private readonly IKernel _kernel;

            public TaskFactory(IKernel kernel)
            {
                _kernel = kernel;
            }

            public ITask Get<TTask>() where TTask : ITask
            {
                return GetByName(typeof (TTask).Name);
            }

            public ITask GetByName(string name)
            {
                if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", name);

                if (!_kernel.HasComponent(name))
                    throw new TaskNotFoundException(name);

                return _kernel.Resolve<ITask>(name);
            }

            public ITask[] GetAll()
            {
                return _kernel.ResolveAll<ITask>().OrderBy(x => x.Name()).ToArray();
            }
        }
	}
}