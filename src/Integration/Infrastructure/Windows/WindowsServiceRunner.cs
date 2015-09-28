using System;
using System.ServiceProcess;

namespace Vertica.Integration.Infrastructure.Windows
{
    internal class WindowsServiceRunner : ServiceBase
    {
	    private readonly string _name;
	    private readonly Func<IDisposable> _onStartFactory;

	    private IDisposable _current;

        public WindowsServiceRunner(string name, Func<IDisposable> onStartFactory)
        {
	        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");

	        _name = name;
	        _onStartFactory = onStartFactory;
        }

	    public new string ServiceName
		{
			get { return _name; }
		}

        protected override void OnStart(string[] args)
        {
            if (_current != null)
                throw new InvalidOperationException("Cannot start when already running.");

	        _current = _onStartFactory();
        }

        protected override void OnStop()
        {
            if (_current != null)
            {
                _current.Dispose();
                _current = null;
            }
        }
    }
}