using Vertica.Integration.Infrastructure;

namespace Experiments.Files
{
	public class ShutdownActions : KernelActions<ShutdownActions>
	{
		protected override ShutdownActions This => this;
	}
}