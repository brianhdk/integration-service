using System;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Model.Tasks
{
    public class ConcurrentTaskExecutionConfiguration
    {
        private readonly ScanAddRemoveInstaller<IPreventConcurrentTaskExecutionRuntimeEvaluator> _evaluators;
        private readonly ScanAddRemoveInstaller<IPreventConcurrentTaskExecutionCustomLockName> _customLockNames;
        private readonly ScanAddRemoveInstaller<IPreventConcurrentTaskExecutionCustomLockDescription> _customLockDescriptions;
        private readonly ScanAddRemoveInstaller<IPreventConcurrentTaskExecutionExceptionHandler> _exceptionHandlers;

        internal ConcurrentTaskExecutionConfiguration(TasksConfiguration tasks)
        {
            if (tasks == null) throw new ArgumentNullException(nameof(tasks));

            _evaluators = new ScanAddRemoveInstaller<IPreventConcurrentTaskExecutionRuntimeEvaluator>(serviceDescriptor: x => x.Self());
            _customLockNames = new ScanAddRemoveInstaller<IPreventConcurrentTaskExecutionCustomLockName>(serviceDescriptor: x => x.Self());
            _customLockDescriptions = new ScanAddRemoveInstaller<IPreventConcurrentTaskExecutionCustomLockDescription>(serviceDescriptor: x => x.Self());
            _exceptionHandlers = new ScanAddRemoveInstaller<IPreventConcurrentTaskExecutionExceptionHandler>(serviceDescriptor: x => x.Self());

            // scan own assembly
            AddFromAssemblyOfThis<ConcurrentTaskExecutionConfiguration>();

            Tasks = tasks.Change(t => t.Application
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Install(_evaluators)
                        .Install(_customLockNames)
                        .Install(_customLockDescriptions)
                        .Install(_exceptionHandlers))));
        }

        public TasksConfiguration Tasks { get; }

        /// <summary>
        /// Scans the assembly of the defined <typeparamref name="T"></typeparamref> for public classes inheriting <see cref="IPreventConcurrentTaskExecutionRuntimeEvaluator"/>./>
        /// <para />
        /// </summary>
        /// <typeparam name="T">The type in which its assembly will be scanned.</typeparam>
        public ConcurrentTaskExecutionConfiguration AddFromAssemblyOfThis<T>()
        {
            _evaluators.AddFromAssemblyOfThis<T>();
            _customLockNames.AddFromAssemblyOfThis<T>();
            _customLockDescriptions.AddFromAssemblyOfThis<T>();
            _exceptionHandlers.AddFromAssemblyOfThis<T>();

            return this;
        }

        /// <summary>
        /// Adds the specified <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Specifies the <see cref="IPreventConcurrentTaskExecutionRuntimeEvaluator"/> to be added.</typeparam>
        public ConcurrentTaskExecutionConfiguration AddRuntimeEvaluator<T>()
            where T : IPreventConcurrentTaskExecutionRuntimeEvaluator
        {
            _evaluators.Add<T>();

            return this;
        }

        /// <summary>
        /// Skips the specified <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">Specifies the <see cref="IPreventConcurrentTaskExecutionRuntimeEvaluator"/> that will be skipped.</typeparam>
        public ConcurrentTaskExecutionConfiguration RemoveRuntimeEvaluator<T>()
            where T : IPreventConcurrentTaskExecutionRuntimeEvaluator
        {
            _evaluators.Remove<T>();

            return this;
        }

        /// <summary>
        /// Adds the specified <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Specifies the <see cref="IPreventConcurrentTaskExecutionCustomLockName"/> to be added.</typeparam>
        public ConcurrentTaskExecutionConfiguration AddCustomLockName<T>()
            where T : IPreventConcurrentTaskExecutionCustomLockName
        {
            _customLockNames.Add<T>();

            return this;
        }

        /// <summary>
        /// Skips the specified <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">Specifies the <see cref="IPreventConcurrentTaskExecutionCustomLockName"/> that will be skipped.</typeparam>
        public ConcurrentTaskExecutionConfiguration RemoveCustomLockName<T>()
            where T : IPreventConcurrentTaskExecutionCustomLockName
        {
            _customLockNames.Remove<T>();

            return this;
        }

        /// <summary>
        /// Adds the specified <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Specifies the <see cref="IPreventConcurrentTaskExecutionCustomLockDescription"/> to be added.</typeparam>
        public ConcurrentTaskExecutionConfiguration AddCustomLockDescription<T>()
            where T : IPreventConcurrentTaskExecutionCustomLockDescription
        {
            _customLockDescriptions.Add<T>();

            return this;
        }

        /// <summary>
        /// Skips the specified <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">Specifies the <see cref="IPreventConcurrentTaskExecutionCustomLockDescription"/> that will be skipped.</typeparam>
        public ConcurrentTaskExecutionConfiguration RemoveCustomLockDescription<T>()
            where T : IPreventConcurrentTaskExecutionCustomLockDescription
        {
            _customLockDescriptions.Remove<T>();

            return this;
        }

        /// <summary>
        /// Adds the specified <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Specifies the <see cref="IPreventConcurrentTaskExecutionExceptionHandler"/> to be added.</typeparam>
        public ConcurrentTaskExecutionConfiguration AddExceptionHandler<T>()
            where T : IPreventConcurrentTaskExecutionExceptionHandler
        {
            _exceptionHandlers.Add<T>();

            return this;
        }

        /// <summary>
        /// Skips the specified <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">Specifies the <see cref="IPreventConcurrentTaskExecutionExceptionHandler"/> that will be skipped.</typeparam>
        public ConcurrentTaskExecutionConfiguration RemoveExceptionHandler<T>()
            where T : IPreventConcurrentTaskExecutionExceptionHandler
        {
            _exceptionHandlers.Remove<T>();

            return this;
        }

        /// <summary>
        /// Clears all registrations.
        /// </summary>
        public ConcurrentTaskExecutionConfiguration Clear()
        {
            _evaluators.Clear();
            _customLockNames.Clear();
            _customLockDescriptions.Clear();
            _exceptionHandlers.Clear();

            return this;
        }
    }
}