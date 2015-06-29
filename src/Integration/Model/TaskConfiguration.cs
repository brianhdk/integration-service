using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Model
{
    public abstract class TaskConfiguration : IEquatable<TaskConfiguration>
    {
        protected TaskConfiguration(Type task)
        {
            if (task == null) throw new ArgumentNullException("task");

            Task = task;
            Steps = new List<Type>();
        }

        internal Type Task { get; private set; }
        protected List<Type> Steps { get; private set; }

        internal abstract IWindsorInstaller GetInstaller();

        public bool Equals(TaskConfiguration other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Task == other.Task;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TaskConfiguration) obj);
        }

        public override int GetHashCode()
        {
            return Task.GetHashCode();
        }
    }

    public sealed class TaskConfiguration<TWorkItem> : TaskConfiguration
    {
        internal TaskConfiguration(Type task)
            : base(task)
        {
        }

        public TaskConfiguration<TWorkItem> Step<TStep>()
            where TStep : IStep<TWorkItem>
        {
            Steps.Add(typeof (TStep));

            return this;
        }

        public TaskConfiguration<TWorkItem> Clear()
        {
            Steps.Clear();

            return this;
        }

        public TaskConfiguration<TWorkItem> Remove<TStep>()
            where TStep : IStep<TWorkItem>
        {
            Steps.RemoveAll(x => x == typeof (TStep));

            return this;
        }

        internal override IWindsorInstaller GetInstaller()
        {
            return new TaskInstaller<TWorkItem>(Task, Steps);
        }
    }
}