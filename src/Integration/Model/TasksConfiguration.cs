using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Model
{
    public class TasksConfiguration
    {
        private readonly List<Assembly> _scan;
        private readonly List<Type> _add; 
        private readonly List<Type> _skip;
        private readonly List<TaskConfiguration> _tasks;

        internal TasksConfiguration()
        {
            _scan = new List<Assembly>();
            _add = new List<Type>();
            _skip = new List<Type>();
            _tasks = new List<TaskConfiguration>();

            // scan own assembly
            AddTasksFromAssemblyOfThis<TasksConfiguration>();
        }

        /// <summary>
        /// Scans the assembly of the defined <typeparamref name="T"></typeparamref> for public classes inheriting <see cref="Task"/>.
        /// <para />
        /// Note: This will <u>not</u> register classes that make use of WorkItem and Steps (<see cref="Task{TWorkItem}"/>).
        /// For that you have to use the <see cref="Add{TTask, TWorkItem}"/> method to explicitly define the Task and its ordered Steps.
        /// </summary>
        /// <typeparam name="T">The type in which its assembly will be scanned.</typeparam>
        public TasksConfiguration AddTasksFromAssemblyOfThis<T>()
        {
            _scan.Add(typeof(T).Assembly);

            return this;
        }

        /// <summary>
        /// Adds the specified <typeparamref name="TTask"/>.
        /// </summary>
        /// <typeparam name="TTask">Specifies the <see cref="Task{TWorkItem}"/> to be added.</typeparam>
        /// <typeparam name="TWorkItem">Specifies the WorkItem that is used by this Task.</typeparam>
        /// <param name="task">Required in order to register one or more <see cref="Step{TWorkItem}"/></param> sequentially executed by this Task.
        public TasksConfiguration Add<TTask, TWorkItem>(Action<TaskConfiguration<TWorkItem>> task)
            where TTask : Task<TWorkItem>
        {
            if (task == null) throw new ArgumentNullException("task");

            var configuration = new TaskConfiguration<TWorkItem>(typeof (TTask));

            task(configuration);

            if (_tasks.Contains(configuration))
                throw new NotSupportedException(
                    String.Format(@"Task '{0}' has already been added.", configuration.Task.Name));

            _tasks.Add(configuration);

            return this;
        }

        /// <summary>
        /// Adds the specified <typeparamref name="TTask" />.
        /// </summary>
        /// <typeparam name="TTask">Specifies the <see cref="Task"/> to be added.</typeparam>
        public TasksConfiguration Add<TTask>()
            where TTask : Task
        {
            _add.Add(typeof(TTask));

            return this;
        }

        /// <summary>
        /// Skips the specified <typeparamref name="TTask" />.
        /// </summary>
        /// <typeparam name="TTask">Specifies the <see cref="Task"/> that will be skipped.</typeparam>
        public TasksConfiguration Skip<TTask>()
            where TTask : Task
        {
            _skip.Add(typeof(TTask));

            return this;
        }

        internal void Install(IWindsorContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");

            container.Install(new TaskInstaller(ScanAssemblies, AddTypes, SkipTypes));

            foreach (TaskConfiguration task in _tasks.Distinct())
                container.Install(task.GetInstaller());

            container.Install(new TaskFactoryInstaller());
        }

        private Assembly[] ScanAssemblies
        {
            get { return _scan.Distinct().ToArray(); }
        }

        private Type[] AddTypes
        {
            get { return _add.Distinct().Except(SkipTypes).ToArray(); }
        }

        private Type[] SkipTypes
        {
            get { return _skip.Distinct().ToArray(); }
        }

        /// <summary>
        /// Removes all registred Tasks including the built-in Tasks (MigrateTask, WriteDocumentationTask etc.)."/>
        /// </summary>
        public TasksConfiguration RemoveAll()
        {
            _scan.Clear();
            _add.Clear();
            _skip.Clear();

            return this;
        }
    }
}