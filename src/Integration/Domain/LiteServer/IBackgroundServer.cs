using System.Threading;
using System.Threading.Tasks;

namespace Vertica.Integration.Domain.LiteServer
{
	/// <summary>
	/// With a <see cref="IBackgroundServer"/> you control everything from instantiating the TPL task, to the graceful shutdown of your Task when the CancellationToken is requested to be canceled.
	/// </summary>
	public interface IBackgroundServer
	{
		Task Create(CancellationToken token, BackgroundServerContext context);
	}
}