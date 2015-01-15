namespace Vertica.Integration.Model
{
	public interface IStep
	{
		string Description { get; }
	}

	public interface IStep<in TWorkItem> : IStep
	{
		Execution ContinueWith(TWorkItem workItem);
		void Execute(TWorkItem workItem, Log log);
	}
}