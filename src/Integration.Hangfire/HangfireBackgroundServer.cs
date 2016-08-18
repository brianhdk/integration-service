using System.Threading;
using System.Threading.Tasks;
using Vertica.Integration.Domain.LiteServer;

namespace Vertica.Integration.Hangfire
{
	internal class HangfireBackgroundServer : IBackgroundServer
	{
		private readonly IHangfireServerFactory _factory;

		public HangfireBackgroundServer(IHangfireServerFactory factory)
		{
			_factory = factory;
		}

		public Task Create(CancellationToken token, BackgroundServerContext context)
		{
			return Task.Run(() =>
			{
				using (_factory.Create())
				{
					token.WaitHandle.WaitOne();
				}

			}, token);
		}
	}
}