using System;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Model.Tasks
{
    public class ConcurrentTaskExecutionConfiguration : IInitializable<IWindsorContainer>
    {
        private readonly ScanAddRemoveInstaller<IPreventConcurrentTaskExecutionRuntimeEvaluator> _evaluators;

        internal ConcurrentTaskExecutionConfiguration(TasksConfiguration tasks)
        {
            if (tasks == null) throw new ArgumentNullException(nameof(tasks));

            _evaluators = new ScanAddRemoveInstaller<IPreventConcurrentTaskExecutionRuntimeEvaluator>(serviceDescriptor: x => x.Self());

            // scan own assembly
            AddFromAssemblyOfThis<ConcurrentTaskExecutionConfiguration>();
        }

        /// <summary>
        /// Scans the assembly of the defined <typeparamref name="T"></typeparamref> for public classes inheriting <see cref="IPreventConcurrentTaskExecutionRuntimeEvaluator"/>./>
        /// <para />
        /// </summary>
        /// <typeparam name="T">The type in which its assembly will be scanned.</typeparam>
        public ConcurrentTaskExecutionConfiguration AddFromAssemblyOfThis<T>()
        {
            _evaluators.AddFromAssemblyOfThis<T>();

            return this;
        }

        /// <summary>
        /// Adds the specified <typeparamref name="TEvaluator"/>.
        /// </summary>
        /// <typeparam name="TEvaluator">Specifies the <see cref="IPreventConcurrentTaskExecutionRuntimeEvaluator"/> to be added.</typeparam>
        public ConcurrentTaskExecutionConfiguration AddEvaluator<TEvaluator>()
            where TEvaluator : IPreventConcurrentTaskExecutionRuntimeEvaluator
        {
            _evaluators.Add<TEvaluator>();

            return this;
        }

        /// <summary>
        /// Skips the specified <typeparamref name="TEvaluator" />.
        /// </summary>
        /// <typeparam name="TEvaluator">Specifies the <see cref="IPreventConcurrentTaskExecutionRuntimeEvaluator"/> that will be skipped.</typeparam>
        public ConcurrentTaskExecutionConfiguration RemoveEvaluator<TEvaluator>()
            where TEvaluator : IPreventConcurrentTaskExecutionRuntimeEvaluator
        {
            _evaluators.Remove<TEvaluator>();

            return this;
        }

        /// <summary>
        /// Clears all registrations.
        /// </summary>
        public ConcurrentTaskExecutionConfiguration Clear()
        {
            _evaluators.Clear();

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
            container.Install(_evaluators);
        }
    }
}