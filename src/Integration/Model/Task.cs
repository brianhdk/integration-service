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
	    protected Task(IEnumerable<IStep<TWorkItem>> steps)
		{
			Steps = steps ?? Enumerable.Empty<IStep<TWorkItem>>();
		}

		[JsonProperty(Order = 1)]
		public string Name => this.Name();

		[JsonProperty(Order = 2)]
		public abstract string Description { get; }

		[JsonProperty(Order = 3)]
		public IEnumerable<IStep<TWorkItem>> Steps { get; }

	    IEnumerable<IStep> ITask.Steps => Steps;

		public abstract TWorkItem Start(ITaskExecutionContext context);

        [Obsolete("Override End(ITaskExecution<TWorkItem> instead.")]
	    public virtual void End(TWorkItem workItem, ITaskExecutionContext context)
	    {
	    }

	    public virtual void End(ITaskExecutionContext<TWorkItem> context)
	    {
	    }
	}
}