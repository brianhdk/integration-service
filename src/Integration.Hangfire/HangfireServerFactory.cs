using System;
using Castle.MicroKernel;

namespace Vertica.Integration.Hangfire
{
    internal class HangfireServerFactory : IHangfireServerFactory
    {
        private readonly IKernel _kernel;
        private readonly InternalConfiguration _configuration;

        public HangfireServerFactory(IKernel kernel, InternalConfiguration configuration)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IDisposable Create()
        {
            return new HangfireServerImpl(_kernel, _configuration);
        }
    }
}