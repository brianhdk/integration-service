using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Model
{
    public class TasksConfiguration : IInitializable<IWindsorContainer>
    {
        private readonly List<Assembly> _scan;
        private readonly List<Type> _simpleTasks; 
        private readonly List<Type> _removeTasks;
        private readonly List<TaskConfiguration> _complexTasks;

        internal TasksConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

			Application = application
				.Hosts(hosts => hosts.Host<TaskHost>());

			_scan = new List<Assembly>();
            _simpleTasks = new List<Type>();
            _removeTasks = new List<Type>();
            _complexTasks = new List<TaskConfiguration>();

            // scan own assembly
            AddFromAssemblyOfThis<TasksConfiguration>();
		}

        public ApplicationConfiguration Application { get; }

        /// <summary>
        /// Scans the assembly of the defined <typeparamref name="T"></typeparamref> for public classes inheriting <see cref="Task"/>.
        /// <para />
        /// Note: This will <u>not</u> register classes that make use of WorkItem and Steps (<see cref="Model.Task{TWorkItem}"/>).
        /// For that you have to use the <see cref="Task{TTask,TWorkItem}"/> method to explicitly define the Task and its ordered Steps.
        /// </summary>
        /// <typeparam name="T">The type in which its assembly will be scanned.</typeparam>
        public TasksConfiguration AddFromAssemblyOfThis<T>()
        {
            _scan.Add(typeof(T).Assembly);

            return this;
        }

        /// <summary>
        /// Adds the specified <typeparamref name="TTask"/>.
        /// </summary>
        /// <typeparam name="TTask">Specifies the <see cref="Model.Task{TWorkItem}"/> to be added.</typeparam>
        /// <typeparam name="TWorkItem">Specifies the WorkItem that is used by this Task.</typeparam>
        /// <param name="task">Required in order to register one or more <see cref="Step{TWorkItem}"/></param> sequentially executed by this Task.
        public TasksConfiguration Task<TTask, TWorkItem>(Action<TaskConfiguration<TWorkItem>> task = null)
            where TTask : Task<TWorkItem>
        {
            var configuration = new TaskConfiguration<TWorkItem>(typeof (TTask));

            if (task != null)
                task(configuration);

            if (_complexTasks.Contains(configuration))
                throw new InvalidOperationException(
	                $@"Task '{configuration.Task.FullName}' has already been added.");

            _complexTasks.Add(configuration);

            return this;
        }

        /// <summary>
        /// Adds the specified <typeparamref name="TTask" />.
        /// </summary>
        /// <typeparam name="TTask">Specifies the <see cref="Task"/> to be added.</typeparam>
        public TasksConfiguration Task<TTask>()
            where TTask : Task
        {
            _simpleTasks.Add(typeof(TTask));

            return this;
        }

        /// <summary>
        /// Skips the specified <typeparamref name="TTask" />.
        /// </summary>
        /// <typeparam name="TTask">Specifies the <see cref="Task"/> that will be skipped.</typeparam>
        public TasksConfiguration Remove<TTask>()
            where TTask : Task
        {
            _removeTasks.Add(typeof(TTask));

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
            container.Install(new TaskInstaller(_scan.ToArray(), _simpleTasks.ToArray(), _removeTasks.ToArray()));

            foreach (TaskConfiguration task in _complexTasks.Distinct())
                container.Install(task.GetInstaller());

            container.Install(new TaskFactoryInstaller());
        }

        /// <summary>
        /// Clears all registred Tasks including the built-in Tasks (MigrateTask, WriteDocumentationTask etc.).
        /// </summary>
        public TasksConfiguration Clear()
        {
            _scan.Clear();
            _simpleTasks.Clear();
            _removeTasks.Clear();

            return this;
        }
    }
}