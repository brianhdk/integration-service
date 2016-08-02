using System.Threading;
using System.Threading.Tasks;

namespace Experiments.Files
{
	/// <summary>
	/// With a background operation you control everything from instantiating the TPL task, to the graceful shutdown when the CancellationToken is requested to be cancelled.
	/// </summary>
	public interface IBackgroundOperation
	{
		Task Create(CancellationToken token);
	}
}