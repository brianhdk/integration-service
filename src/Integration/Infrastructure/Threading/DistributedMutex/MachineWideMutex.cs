using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using Vertica.Utilities;

namespace Vertica.Integration.Infrastructure.Threading.DistributedMutex
{
    /// <summary>
    /// http://stackoverflow.com/questions/229565/what-is-a-good-pattern-for-using-a-global-mutex-in-c
    /// </summary>
    /*public class MachineWideMutex : IDistributedMutex
    {
        private readonly IShutdown _shutdown;
        private readonly string _mutexId;
        private readonly MutexSecurity _securitySettings;

        public MachineWideMutex(IShutdown shutdown)
        {
            _shutdown = shutdown;

            // get application GUID as defined in AssemblyInfo.cs
            var appGuid = ((GuidAttribute) Assembly
                .GetEntryAssembly()
                .GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0))
                .Value;

            // unique id for global mutex - Global prefix means it is global to the machine
            _mutexId = $"Global\\{{{appGuid}}}";

            var identifier = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

            var allowEveryoneRule = new MutexAccessRule(identifier, MutexRights.FullControl, AccessControlType.Allow);
            _securitySettings = new MutexSecurity();
            _securitySettings.AddAccessRule(allowEveryoneRule);
        }

        public IDisposable Enter(DistributedMutexContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return new EnterLock(context, _mutexId, _securitySettings, _shutdown.Token);
        }

        private class EnterLock : IDisposable
        {
            private readonly Mutex _mutex;
            private readonly bool _hasHandle;

            public EnterLock(DistributedMutexContext context, string id, MutexSecurity security, CancellationToken cancellationToken)
            {
                bool createdNew;
                _mutex = new Mutex(false, id, out createdNew, security);

                try
                {
                    _hasHandle = _mutex.WaitOne(context.WaitTime, false);

                    if (!_hasHandle)
                        throw new DistributedMutexTimeoutException($"Unable to acquire lock '{context.Name}' within wait time ({context.WaitTime}).");
                }
                catch (AbandonedMutexException)
                {
                    _hasHandle = true;
                }
            }

            public void Dispose()
            {
                if (_hasHandle)
                    _mutex.ReleaseMutex();

                _mutex.Dispose();
            }
        }
    }*/
}