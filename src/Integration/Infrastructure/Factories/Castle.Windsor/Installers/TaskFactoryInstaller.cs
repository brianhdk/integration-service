using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Exceptions;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
    internal class TaskFactoryInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
            container.Register(
                Component
                    .For<ITaskFactory>()
                    .AsFactory());

			container.Register(
				Component
					.For<ITask>()
					.Named("TaskByName")
					.UsingFactoryMethod((kernel, context) =>
					{
						Expression<Action<ITaskFactory>> expression = x => x.GetTaskByName(null);

						MethodInfo method = ((MethodCallExpression)expression.Body).Method;
						string name = method.GetParameters().Select(x => x.Name).Single();

					    if (context.AdditionalArguments.Count == 0)
					        return kernel.Resolve<ITask>();

						var taskName = context.AdditionalArguments[name] as string;

					    if (String.IsNullOrWhiteSpace(taskName))
					        throw new ArgumentException(@"Value cannot be null or empty.", name);
                        
                        if (kernel.HasComponent(taskName))
							return kernel.Resolve<ITask>(taskName);

                        throw new TaskNotFoundException(taskName);
					})
					.LifestyleTransient());
		}
	}
}