using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Domain.LiteServer
{
	public class ShutdownActions : KernelActions<ShutdownActions>
	{
		protected override ShutdownActions This => this;
	}
}