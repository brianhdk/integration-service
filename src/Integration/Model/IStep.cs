namespace Vertica.Integration.Model
{
	public interface IStep
	{
		string Description { get; }
	}

	public interface IStep<in TWorkItem> : IStep
	{
	    Execution ContinueWith(TWorkItem workItem, ITaskExecutionContext context);

        void Execute(TWorkItem workItem, ITaskExecutionContext context);
	}
}