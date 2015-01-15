namespace Vertica.Integration.Model
{
    public class TaskExecutionResult
    {
        public TaskExecutionResult(string[] output)
        {
            Output = output;
        }

        public string[] Output { get; private set; }
    }
}