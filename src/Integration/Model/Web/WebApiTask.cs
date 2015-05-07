namespace Vertica.Integration.Model.Web
{
    public class WebApiTask : Task
    {
        public override string Description
        {
            get { return "WebApi task."; }
        }

        public override void StartTask(ILog log, params string[] arguments)
        {
        }
    }
}