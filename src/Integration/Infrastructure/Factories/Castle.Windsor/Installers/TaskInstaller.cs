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
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
    internal class TaskInstaller : IWindsorInstaller
    {
        private readonly Assembly[] _scanAssemblies;
        private readonly Type[] _addTasks;
        private readonly Type[] _ignoreTasks;

        public TaskInstaller(Assembly[] scanAssemblies, Type[] addTasks, Type[] ignoreTasks)
        {
            _scanAssemblies = scanAssemblies ?? new Assembly[0];
            _addTasks = addTasks ?? new Type[0];
            _ignoreTasks = ignoreTasks ?? new Type[0];
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            foreach (Assembly assembly in _scanAssemblies.Distinct())
            {
                container.Register(
                    Classes.FromAssembly(assembly)
                        .BasedOn<Task>()
                        .Unless(_ignoreTasks.Contains)
                        .Configure(configure => { configure.Named(configure.Implementation.Name); })
                        .WithServiceDefaultInterfaces());
            }

            foreach (Type addType in _addTasks.Except(_ignoreTasks).Distinct())
            {
                container.Register(
                    Component.For<Task>()
                        .ImplementedBy(addType)
                        .Named(addType.Name));
            }
        }
    }

    internal class TaskInstaller<TWorkItem> : IWindsorInstaller
    {
        private readonly Type _task;
        private readonly IEnumerable<Type> _steps;

        public TaskInstaller(Type task, IEnumerable<Type> steps)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (steps == null) throw new ArgumentNullException("steps");

            _task = task;
            _steps = steps;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For(typeof(ITask))
                    .ImplementedBy(_task)
                    .Named(_task.Name));

            var names = new List<string>();

            foreach (Type step in _steps)
            {
                string name = String.Format("{0}.{1}.{2}", _task.Name, step.Name, Guid.NewGuid().ToString("N"));

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
                if (kernel == null) throw new ArgumentNullException("kernel");
                if (stepNames == null) throw new ArgumentNullException("stepNames");

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