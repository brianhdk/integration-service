using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Hangfire
{
	public class ShutdownActions : KernelActions<ShutdownActions>
	{
		protected override ShutdownActions This => this;
	}
}