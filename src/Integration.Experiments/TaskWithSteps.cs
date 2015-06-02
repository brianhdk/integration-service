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
            get { throw new System.NotImplementedException(); }
        }

        public override TaskWorkItem Start(ITaskExecutionContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}