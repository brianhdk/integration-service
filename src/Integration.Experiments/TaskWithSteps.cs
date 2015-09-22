using System;
using System.Collections.Generic;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class TaskWithSteps : Task<TaskWorkItem>
    {
        public TaskWithSteps(IEnumerable<IStep<TaskWorkItem>> steps) : base(steps)
        {
        }

        public override string Description
        {
			get { throw new NotSupportedException(); }
        }

        public override TaskWorkItem Start(ITaskExecutionContext context)
        {
			throw new NotSupportedException();
        }
    }
}