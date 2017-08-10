﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.MicroKernel;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities;
using Vertica.Utilities.Extensions.StringExt;

namespace Vertica.Integration.Domain.LiteServer
{
    internal class HouseKeeping : IBackgroundWorker, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IUptimeTextGenerator _uptime;
        private readonly InternalConfiguration _configuration;
        private readonly Action<string> _output;
        private readonly BackgroundServerHost[] _servers;
        private readonly HashSet<BackgroundServerHost> _isRunning;
        private readonly Task _task;
        private readonly DateTimeOffset _startedAt;

        public HouseKeeping(IKernel kernel, InternalConfiguration configuration, Action<string> output)
        {
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (output == null) throw new ArgumentNullException(nameof(output));

            _logger = kernel.Resolve<ILogger>();
            _uptime = kernel.Resolve<IUptimeTextGenerator>();
            _configuration = configuration;
            _output = message => output($"[{this}]: {message}");

            TaskScheduler scheduler = TaskScheduler.Current;

            IBackgroundServer[] servers = kernel
                .ResolveAll<IBackgroundWorker>()
                .Select(worker => new BackgroundWorkerServer(worker, scheduler))
                .Concat(kernel.ResolveAll<IBackgroundServer>())
                .ToArray();

            CancellationToken token = kernel.Resolve<IShutdown>().Token;

            _servers = servers
                .Select(server => new BackgroundServerHost(server, scheduler, kernel, output))
                .ToArray();

            // To keep track of which servers are actually running.
            _isRunning = new HashSet<BackgroundServerHost>();

            // Spin off the housekeeping background server
            _task = Start(kernel, scheduler, token);

            // Keep track of when Housekeeping was started.
            _startedAt = Time.UtcNow;
        }

        private Task Start(IKernel kernel, TaskScheduler scheduler, CancellationToken token)
        {
            if (_servers.Length == 0)
                return Task.FromResult(true);

            var server = new BackgroundWorkerServer(this, scheduler);

            Task task = server.Create(new BackgroundServerContext(kernel), token);

            return task;
        }

        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return context.Exit();

            if (context.InvocationCount == 1)
            {
                foreach (BackgroundServerHost host in _servers)
                {
                    try
                    {
                        host.Start();
                        _isRunning.Add(host);
                    }
                    catch (Exception ex)
                    {
                        LogError(host, ex);
                    }
                }

                _output($"Started {_isRunning.Count} server(s)");

                return context.Wait(_configuration.HouseKeepingInterval);
            }

            // Calculates whether to output status text or not, depending on number of iterations.
            bool outputStatus = (context.InvocationCount - 1) % _configuration.HouseKeepingOutputStatusOnNumberOfIterations == 0;

            CheckIsRunning(outputStatus);

            if (_isRunning.Count == 0)
            {
                _output("Exiting (no more servers left to monitor)");
                return context.Exit();
            }

            if (outputStatus)
                _output($"{_isRunning.Count} server(s) running. Uptime: {_uptime.GetUptimeText(_startedAt)}");

            return context.Wait(_configuration.HouseKeepingInterval);
        }

        private void CheckIsRunning(bool outputStatus = true)
        {
            foreach (BackgroundServerHost host in _servers.Where(_isRunning.Contains))
            {
                try
                {
                    string statusText;
                    if (!host.IsRunning(ex => LogError(host, ex), out statusText))
                    {
                        _isRunning.Remove(host);
                        outputStatus = true;
                    }

                    if (!string.IsNullOrWhiteSpace(statusText) && outputStatus)
                        _output($"[{host}]: {statusText}");
                }
                catch (Exception ex)
                {
                    LogError(host, ex);
                    _isRunning.Remove(host);
                }
            }
        }

        private void LogError<TSender>(TSender sender, AggregateException ex)
        {
            foreach (Exception exception in ex.Flatten().InnerExceptions)
                LogError(sender, exception);
        }

        private void LogError<TSender>(TSender sender, Exception exception)
        {
            ErrorLog errorLog = _logger.LogError(exception);

            _output($"[{sender}]: [ERROR (ID: {errorLog?.Id.NullIfEmpty() ?? "<n/a>"})]: {exception.DestructMessage()}");
        }

        public void Dispose()
        {
            _output("Stopping");

            try
            {
                if (_task.Exception != null)
                    throw _task.Exception;

                if (_task.Status == TaskStatus.Running)
                    _task.Wait(_configuration.ShutdownTimeout);
            }
            catch (AggregateException ex)
            {
                LogError(this, ex);
            }

            if (_task.Status != TaskStatus.Running)
                _task.Dispose();

            CheckIsRunning();

            // Allow for servers to finish within a specified timeout.
            foreach (BackgroundServerHost host in _servers.Where(_isRunning.Contains).AsParallel())
            {
                try
                {
                    host.WaitForExit(_configuration.ShutdownTimeout);
                }
                catch (AggregateException ex)
                {
                    LogError(host, ex);
                }
            }

            // Dispose all hosts in parallel
            foreach (BackgroundServerHost host in _servers.AsParallel())
                host.Dispose();

            _output("Stopped");
        }

        public override string ToString()
        {
            return nameof(HouseKeeping);
        }
    }
}