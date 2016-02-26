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

            public bool Exists(string name)
            {
                if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(name));

                IHandler handler = _kernel.GetHandler(name);

                if (handler != null)
                    return typeof(ITask).IsAssignableFrom(handler.ComponentModel.Implementation);

                return false;
            }

            public ITask Get<TTask>() where TTask : class, ITask
            {
                return Get(Task.NameOf<TTask>());
            }

            public ITask Get(string name)
            {
                if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", name);

                if (!Exists(name))
                    throw new TaskNotFoundException(name);

                return _kernel.Resolve<ITask>(name);
            }

	        public bool TryGet(string name, out ITask task)
	        {
		        task = null;

		        if (Exists(name))
			        task = Get(name);

		        return task != null;
	        }

	        public ITask[] GetAll()
            {
                return _kernel.ResolveAll<ITask>().OrderBy(x => x.Name()).ToArray();
            }
        }
	}
}