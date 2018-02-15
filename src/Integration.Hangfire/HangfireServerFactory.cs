using System;
using Castle.MicroKernel;

namespace Vertica.Integration.Hangfire
{
    internal class HangfireServerFactory : IHangfireServerFactory
    {
        private readonly IKernel _kernel;
        private readonly IInternalConfiguration _configuration;

        public HangfireServerFactory(IKernel kernel, IInternalConfiguration configuration)
        {
            _kernel = kernel;
            _configuration = configuration;
        }

        public IDisposable Create()
        {
            return new HangfireServerImpl(_kernel, _configuration);
        }
    }
}