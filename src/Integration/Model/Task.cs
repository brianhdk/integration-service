using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Vertica.Integration.Infrastructure.Extensions;

namespace Vertica.Integration.Model
{
	public abstract class Task : Task<EmptyWorkItem>
	{
		protected Task()
			: base(Enumerable.Empty<IStep<EmptyWorkItem>>())
		{
		}

        public override EmptyWorkItem Start(ITaskExecutionContext context)
		{
            if (context == null) throw new ArgumentNullException(nameof(context));

            StartTask(context);

			return new EmptyWorkItem();
		}

        public abstract void StartTask(ITaskExecutionContext context);

	    public static string NameOf<TTask>() where TTask : ITask
	    {
	        return typeof (TTask).TaskName();
	    }
	}

	public abstract class Task<TWorkItem> : ITask<TWorkItem>
	{
		private readonly IEnumerable<IStep<TWorkItem>> _steps;

		protected Task(IEnumerable<IStep<TWorkItem>> steps)
		{
			_steps = steps ?? Enumerable.Empty<IStep<TWorkItem>>();
		}

		[JsonProperty(Order = 1)]
		public string Name => this.Name();

		[JsonProperty(Order = 2)]
		public abstract string Description { get; }

		[JsonProperty(Order = 3)]
		public IEnumerable<IStep<TWorkItem>> Steps => _steps;

		IEnumerable<IStep> ITask.Steps => Steps;

		public abstract TWorkItem Start(ITaskExecutionContext context);

        public virtual void End(TWorkItem workItem, ITaskExecutionContext context)
		{
		}
	}
}