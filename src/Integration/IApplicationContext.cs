using System;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration
{
    public interface IApplicationContext : IDisposable
    {
        void Execute(params string[] args);
        void Execute(HostArguments args);
    }
}