using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Exceptions;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
    internal class TaskInstaller : IWindsorInstaller
    {
        private readonly Assembly[] _scan;
        private readonly Type[] _add;
        private readonly Type[] _ignore;

        public TaskInstaller(Assembly[] scanAssemblies, Type[] addTasks, Type[] ignoreTasks)
        {
            _scan = scanAssemblies ?? new Assembly[0];
            _add = addTasks ?? new Type[0];
            _ignore = ignoreTasks ?? new Type[0];
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            foreach (Assembly assembly in _scan.Distinct())
            {
                container.Register(
                    Classes.FromAssembly(assembly)
                        .BasedOn<Task>()
						.Unless(x =>
						{
							if (_ignore.Contains(x) || _add.Contains(x))
								return true;

							return false;
						})
                        .Configure(x =>
                        {
                            string name = x.Implementation.TaskName();

                            if (container.Kernel.HasComponent(name))
                                throw new TaskWithSameNameAlreadyRegistredException(x.Implementation);

                            x.Named(name);
                        })
                        .WithServiceDefaultInterfaces());
            }

            foreach (Type addType in _add.Except(_ignore).Distinct())
            {
                try
                {
                    container.Register(
                        Component.For<ITask>()
                            .ImplementedBy(addType)
                            .Named(addType.TaskName()));
                }
                catch (ComponentRegistrationException ex)
                {
                    throw new TaskWithSameNameAlreadyRegistredException(addType, ex);
                }
            }
        }
    }

    internal class TaskInstaller<TWorkItem> : IWindsorInstaller
    {
        private readonly Type _task;
        private readonly IEnumerable<Type> _steps;

        public TaskInstaller(Type task, IEnumerable<Type> steps)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (steps == null) throw new ArgumentNullException(nameof(steps));

            _task = task;
            _steps = steps;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            try
            {
                container.Register(
                    Component.For(typeof(ITask))
                        .ImplementedBy(_task)
                        .Named(_task.TaskName()));
            }
            catch (ComponentRegistrationException ex)
            {
                throw new TaskWithSameNameAlreadyRegistredException(_task, ex);
            }

            var names = new List<string>();

            foreach (Type step in _steps)
            {
                string name = $"{_task.TaskName()}.{step.StepName()}.{Guid.NewGuid().ToString("N")}";

                container.Register(
                    Component.For<IStep<TWorkItem>>()
                        .ImplementedBy(step)
                        .Named(name));

                names.Add(name);
            }

            container.Kernel.Resolver.AddSubResolver(new TaskStepsResolver(container.Kernel, _task, names.ToArray()));
        }

        private class TaskStepsResolver : ISubDependencyResolver
        {
            private readonly IKernel _kernel;
            private readonly Type _task;
            private readonly string[] _stepNames;

            public TaskStepsResolver(IKernel kernel, Type task, string[] stepNames)
            {
                if (kernel == null) throw new ArgumentNullException(nameof(kernel));
                if (stepNames == null) throw new ArgumentNullException(nameof(stepNames));

                _kernel = kernel;
                _task = task;
                _stepNames = stepNames;
            }

            public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
            {
                return 
                    model.Implementation == _task &&
                    dependency.TargetItemType == typeof(IEnumerable<IStep<TWorkItem>>);
            }

            public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
            {
                return _stepNames.Select(x => _kernel.Resolve<IStep<TWorkItem>>(x)).ToArray();
            }
        }
    }
}