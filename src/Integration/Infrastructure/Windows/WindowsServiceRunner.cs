using System;
using System.ServiceProcess;

namespace Vertica.Integration.Infrastructure.Windows
{
    public class WindowsServiceRunner : ServiceBase
    {
        private readonly Func<IDisposable> _actionFactory;
        private readonly string _name;

        private IDisposable _currentAction;

        public WindowsServiceRunner(string name, Func<IDisposable> actionFactory)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");
            if (actionFactory == null) throw new ArgumentNullException("actionFactory");

            _name = name;
            _actionFactory = actionFactory;
        }

        public new string ServiceName
        {
            get { return WindowsServiceInstaller.GetServiceName(_name); }
        }

        protected override void OnStart(string[] args)
        {
            if (_currentAction != null)
                throw new InvalidOperationException("Cannot start when already running.");

            _currentAction = _actionFactory();
        }

        protected override void OnStop()
        {
            if (_currentAction != null)
            {
                _currentAction.Dispose();
                _currentAction = null;
            }
        }
    }
}