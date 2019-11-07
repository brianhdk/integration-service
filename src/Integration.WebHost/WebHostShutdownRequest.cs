using System.Threading;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.WebHost
{
    internal class WebHostShutdownRequest : IWaitForShutdownRequest
    {
        private readonly CancellationToken _token;

        public WebHostShutdownRequest(CancellationToken token)
        {
            _token = token;
        }

        public void Wait()
        {
            _token.WaitHandle.WaitOne();
        }
    }
}