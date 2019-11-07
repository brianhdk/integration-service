using System;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.ConsoleHost
{
    internal class ConsoleHostConfiguration
    {
        internal ConsoleHostConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

            Application = application
                .Services(services => services
                    .Interceptors(interceptors => interceptors
                        .InterceptService<IWaitForShutdownRequest, WindowsServiceInterceptor>()
                        .InterceptService<IHost, WindowsServiceInterceptor>()));
        }

        public ApplicationConfiguration Application { get; }
    }
}