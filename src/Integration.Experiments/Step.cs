using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class Step : Step<TaskWorkItem>
    {
        public override string Description
        {
            get { throw new System.NotImplementedException(); }
        }

        public override void Execute(TaskWorkItem workItem, ITaskExecutionContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}