using System;
using System.Threading;
using System.Threading.Tasks;
using Rebus.Bus;
using Vertica.Integration.Domain.LiteServer;

namespace Vertica.Integration.Rebus
{
	internal class RebusBackgroundServer : IBackgroundServer
	{
	    private readonly Func<IBus> _bus;

        public RebusBackgroundServer(Func<IBus> bus)
        {
            _bus = bus;
        }

		public Task Create(BackgroundServerContext context, CancellationToken token)
		{
			return Task.Run(() =>
			{
			    _bus();
				token.WaitHandle.WaitOne();

			}, token);
		}

	    public override string ToString()
	    {
	        return nameof(RebusBackgroundServer);
	    }
	}
}