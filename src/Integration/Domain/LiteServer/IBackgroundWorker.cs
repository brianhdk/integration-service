using System.Threading;

namespace Vertica.Integration.Domain.LiteServer
{
	/// <summary>
	/// With a <see cref="IBackgroundWorker"/> we'll help you out with creating and starting the TPL task, you just need to implement what should happen in the Work(...) method - and return a wait-time (TimeSpan) for when we should execute your method again.
	/// We'll make sure to perform the graceful shutdown, but if your code is running for too long (more than 5 seconds (configurable)) - you'll be shutdown effectively by us.
	/// </summary>
	public interface IBackgroundWorker
	{
		BackgroundWorkerContinuation Work(CancellationToken token, BackgroundWorkerContext context);
	}
}