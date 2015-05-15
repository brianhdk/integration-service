using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class TaskOutputtingHi : Task
    {
        public override string Description
        {
            get { return "TBD"; }
        }

        public override void StartTask(ILog log, params string[] arguments)
        {
            log.Message("Hi from Task.");
        }
    }
}