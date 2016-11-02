using System;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Model.Tasks
{
    public class ConcurrentTaskExecutionConfiguration
    {
        private readonly ScanAddRemoveInstaller<IPreventConcurrentTaskExecutionRuntimeEvaluator> _evaluators;
        private readonly ScanAddRemoveInstaller<IPreventConcurrentTaskExecutionCustomLockName> _customLockNames;

        internal ConcurrentTaskExecutionConfiguration(TasksConfiguration tasks)
        {
            if (tasks == null) throw new ArgumentNullException(nameof(tasks));

            _evaluators = new ScanAddRemoveInstaller<IPreventConcurrentTaskExecutionRuntimeEvaluator>(serviceDescriptor: x => x.Self());
            _customLockNames = new ScanAddRemoveInstaller<IPreventConcurrentTaskExecutionCustomLockName>(serviceDescriptor: x => x.Self());

            // scan own assembly
            AddFromAssemblyOfThis<ConcurrentTaskExecutionConfiguration>();

            Tasks = tasks.Change(t => t.Application
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Install(_evaluators)
                        .Install(_customLockNames))));
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

            return this;
        }

        /// <summary>
        /// Adds the specified <typeparamref name="TRuntimeEvaluator"/>.
        /// </summary>
        /// <typeparam name="TRuntimeEvaluator">Specifies the <see cref="IPreventConcurrentTaskExecutionRuntimeEvaluator"/> to be added.</typeparam>
        public ConcurrentTaskExecutionConfiguration AddRuntimeEvaluator<TRuntimeEvaluator>()
            where TRuntimeEvaluator : IPreventConcurrentTaskExecutionRuntimeEvaluator
        {
            _evaluators.Add<TRuntimeEvaluator>();

            return this;
        }

        /// <summary>
        /// Skips the specified <typeparamref name="TRuntimeEvaluators" />.
        /// </summary>
        /// <typeparam name="TRuntimeEvaluators">Specifies the <see cref="IPreventConcurrentTaskExecutionRuntimeEvaluator"/> that will be skipped.</typeparam>
        public ConcurrentTaskExecutionConfiguration RemoveRuntimeEvaluator<TRuntimeEvaluators>()
            where TRuntimeEvaluators : IPreventConcurrentTaskExecutionRuntimeEvaluator
        {
            _evaluators.Remove<TRuntimeEvaluators>();

            return this;
        }

        /// <summary>
        /// Adds the specified <typeparamref name="TEvaluator"/>.
        /// </summary>
        /// <typeparam name="TEvaluator">Specifies the <see cref="IPreventConcurrentTaskExecutionCustomLockName"/> to be added.</typeparam>
        public ConcurrentTaskExecutionConfiguration AddCustomLockName<TEvaluator>()
            where TEvaluator : IPreventConcurrentTaskExecutionCustomLockName
        {
            _customLockNames.Add<TEvaluator>();

            return this;
        }

        /// <summary>
        /// Skips the specified <typeparamref name="TEvaluator" />.
        /// </summary>
        /// <typeparam name="TEvaluator">Specifies the <see cref="IPreventConcurrentTaskExecutionCustomLockName"/> that will be skipped.</typeparam>
        public ConcurrentTaskExecutionConfiguration RemoveCustomLockName<TEvaluator>()
            where TEvaluator : IPreventConcurrentTaskExecutionCustomLockName
        {
            _customLockNames.Remove<TEvaluator>();

            return this;
        }

        /// <summary>
        /// Clears all registrations.
        /// </summary>
        public ConcurrentTaskExecutionConfiguration Clear()
        {
            _evaluators.Clear();
            _customLockNames.Clear();

            return this;
        }
    }
}