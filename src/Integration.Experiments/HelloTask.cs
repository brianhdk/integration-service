using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class HelloTask : Task
    {
        public override string Description => "TBD";

	    public override void StartTask(ITaskExecutionContext context)
        {
            context.Log.Message("Hi from Task.");
        }
    }
}