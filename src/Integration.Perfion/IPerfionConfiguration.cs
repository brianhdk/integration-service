using System;
using Castle.MicroKernel;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Perfion.PerfionAPIService;

namespace Vertica.Integration.Perfion
{
    internal interface IPerfionConfiguration
    {
        ConnectionString ConnectionString { get; }
        ArchiveOptions ArchiveOptions { get; }
        ServiceClientConfiguration ServiceClientConfiguration { get; }
        WebClientConfiguration WebClientConfiguration { get; }

        T WithProxy<T>(IKernel kernel, Func<GetDataSoap, T> client);
    }
}