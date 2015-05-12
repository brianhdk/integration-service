namespace Vertica.Integration.Model
{
    public class TaskExecutionResult
    {
        public TaskExecutionResult(string[] output)
        {
            Output = output ?? new string[0];
        }

        public string[] Output { get; private set; }
    }
}