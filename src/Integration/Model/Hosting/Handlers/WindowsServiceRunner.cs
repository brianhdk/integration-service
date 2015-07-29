using System;
using System.ServiceProcess;

namespace Vertica.Integration.Model.Hosting.Handlers
{
    internal class WindowsServiceRunner : ServiceBase
    {
	    private readonly IRuntimeSettings _runtimeSettings;
	    private readonly WindowsService _service;

	    private IDisposable _current;

        public WindowsServiceRunner(IRuntimeSettings runtimeSettings, WindowsService service)
        {
	        if (runtimeSettings == null) throw new ArgumentNullException("runtimeSettings");
	        if (service == null) throw new ArgumentNullException("service");

	        _runtimeSettings = runtimeSettings;
	        _service = service;
        }

	    public new string ServiceName
		{
			get { return WindowsServiceInstaller.GetServiceName(_runtimeSettings, _service); }
		}

        protected override void OnStart(string[] args)
        {
            if (_current != null)
                throw new InvalidOperationException("Cannot start when already running.");

            _current = _service.OnStartFactory();
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