namespace Vertica.Integration.Model.Web
{
    public class WebApiTask : Task
    {
        public override string Description
        {
            get { return "WebApi task."; }
        }

        public override string Schedule
        {
            get { return "Not defined."; }
        }

        public override void StartTask(Log log, params string[] arguments)
        {
        }
    }
}