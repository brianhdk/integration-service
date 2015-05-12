using System;
using System.ServiceProcess;

namespace Vertica.Integration.Infrastructure.Windows
{
    internal class WindowsServiceRunner : ServiceBase
    {
        private readonly Func<IDisposable> _taskFactory;
        private readonly string _name;

        private IDisposable _task;

        public WindowsServiceRunner(string name, Func<IDisposable> taskFactory)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");
            if (taskFactory == null) throw new ArgumentNullException("taskFactory");

            _name = name;
            _taskFactory = taskFactory;
        }

        public new string ServiceName
        {
            get { return WindowsServiceInstaller.GetServiceName(_name); }
        }

        protected override void OnStart(string[] args)
        {
            if (_task != null)
                throw new InvalidOperationException("Cannot start when already running.");

            _task = _taskFactory();
        }

        protected override void OnStop()
        {
            if (_task != null)
            {
                _task.Dispose();
                _task = null;
            }
        }
    }
}