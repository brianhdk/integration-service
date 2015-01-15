using System;
using System.Linq;
using System.Linq.Expressions;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
    public class TaskFactoryInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(
				Component
					.For<ITask>()
					.Named("TaskByName")
					.UsingFactoryMethod((kernel, context) =>
					{
						Expression<Action<ITaskFactory>> expression = o => o.GetTaskByName(null);
						var method = ((MethodCallExpression)expression.Body).Method;
						var name = method.GetParameters().Select(pi => pi.Name).Single();

						var taskName = context.AdditionalArguments[name] as string;

						if (!String.IsNullOrWhiteSpace(taskName))
							return kernel.Resolve<ITask>(taskName);

						return kernel.Resolve<ITask>();
					})
					.LifestyleTransient());

			container.Register(
				Component
					.For<ITaskFactory>()
					.AsFactory());
		}
	}
}