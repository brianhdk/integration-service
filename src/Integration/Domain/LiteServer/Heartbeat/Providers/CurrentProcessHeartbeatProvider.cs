using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Extensions;

namespace Vertica.Integration.Domain.LiteServer.Heartbeat.Providers
{
    public class CurrentProcessHeartbeatProvider : IHeartbeatProvider
    {
        private readonly IUptime _uptime;

        public CurrentProcessHeartbeatProvider(IUptime uptime)
        {
            _uptime = uptime;
        }

        public IEnumerable<string> CollectHeartbeatMessages(CancellationToken token)
        {
            yield return $"CommandLine: {Environment.CommandLine}";

            yield return $"StartedAt: {_uptime.StartedAt}";
            yield return $"Uptime: {_uptime.UptimeText}";

            Process process = Process.GetCurrentProcess();

            yield return $"Total threads: {process.Threads.Count}";

            yield return $"NonpagedSystemMemorySize64: {process.NonpagedSystemMemorySize64.ToPrettyFileSize()}";
            yield return $"PagedMemorySize64: {process.PagedMemorySize64.ToPrettyFileSize()}";
            yield return $"PagedSystemMemorySize64: {process.PagedSystemMemorySize64.ToPrettyFileSize()}";
            yield return $"PeakPagedMemorySize64: {process.PeakPagedMemorySize64.ToPrettyFileSize()}";
            yield return $"PeakVirtualMemorySize64: {process.PeakVirtualMemorySize64.ToPrettyFileSize()}";
            yield return $"PeakWorkingSet64: {process.PeakWorkingSet64.ToPrettyFileSize()}";
            yield return $"PrivateMemorySize64: {process.PrivateMemorySize64.ToPrettyFileSize()}";
            yield return $"VirtualMemorySize64: {process.VirtualMemorySize64.ToPrettyFileSize()}";
            yield return $"WorkingSet64: {process.WorkingSet64.ToPrettyFileSize()}";

            yield return $"PrivilegedProcessorTime: {process.PrivilegedProcessorTime.ToPrettyDuration()}";
            yield return $"TotalProcessorTime: {process.TotalProcessorTime.ToPrettyDuration()}";
            yield return $"UserProcessorTime: {process.UserProcessorTime.ToPrettyDuration()}";
        }
    }
}