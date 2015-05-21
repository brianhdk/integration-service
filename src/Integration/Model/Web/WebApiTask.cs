namespace Vertica.Integration.Model.Web
{
    public class WebApiTask : Task
    {
        public override void StartTask(ITaskExecutionContext context)
        {
        }

        public override string Description
        {
            get { return @"WebApiTask is used to host and expose WebApi ApiControllers registred. To run this Task, use the following command: ""WebApiTask -url http://localhost:8080"" (you can choose any valid URL)."; }
        }
    }
}