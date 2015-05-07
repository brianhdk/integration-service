namespace Vertica.Integration.Model
{
    public abstract class Step<TWorkItem> : IStep<TWorkItem>
    {
        public abstract string Description { get; }

        public virtual Execution ContinueWith(TWorkItem workItem)
        {
            return Execution.Execute;
        }

        public abstract void Execute(TWorkItem workItem, ILog log);
    }
}