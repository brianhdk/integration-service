using System;
using System.Collections;
using System.Collections.Generic;
using Castle.MicroKernel;
using Hangfire;

namespace Vertica.Integration.Hangfire
{
	internal class InternalConfiguration : IInternalConfiguration
	{
	    private readonly List<Action<IGlobalConfiguration, IKernel>> _list;

        public InternalConfiguration()
		{
			ServerOptions = new BackgroundJobServerOptions();
			OnStartup = new StartupActions();
			OnShutdown = new ShutdownActions();
            
		    _list = new List<Action<IGlobalConfiguration, IKernel>>();
        }

		public BackgroundJobServerOptions ServerOptions { get; }
		public StartupActions OnStartup { get; }
		public ShutdownActions OnShutdown { get; }

	    IEnumerator IEnumerable.GetEnumerator()
	    {
	        return GetEnumerator();
	    }

	    public IEnumerator<Action<IGlobalConfiguration, IKernel>> GetEnumerator()
	    {
	        return _list.GetEnumerator();
	    }

	    public void Add(Action<IGlobalConfiguration, IKernel> configuration)
	    {
	        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

	        _list.Add(configuration);
	    }
    }
}