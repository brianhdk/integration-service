using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.Windsor;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Vertica.Integration.WebApi.SignalR.Infrastructure.Castle.Windsor;

namespace Vertica.Integration.WebApi.SignalR
{
    public class SignalRConfiguration : IInitializable<IWindsorContainer>
    {
        private readonly List<Assembly> _assemblies;
	    private readonly List<Type> _removeHubs; 
		private readonly List<Type> _removeModules;

	    internal SignalRConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException("application");

			Application = application;

            _assemblies = new List<Assembly>();
			_removeModules = new List<Type>();
			_removeHubs = new List<Type>();

		    AddFromAssemblyOfThis<SignalRConfiguration>();
        }

        public ApplicationConfiguration Application { get; private set; }

		public SignalRConfiguration AddFromAssemblyOfThis<T>()
        {
            _assemblies.Add(typeof (T).Assembly);

            return this;
        }

	    public SignalRConfiguration RemoveHub<THub>() where THub : Hub
	    {
		    _removeHubs.Add(typeof (THub));

		    return this;
	    }

		public SignalRConfiguration RemovePipeline<THubPipelineModule>() where THubPipelineModule : HubPipelineModule
		{
			_removeModules.Add(typeof (THubPipelineModule));

			return this;
		}

	    void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
	    {
			container.Install(new SignalRInstaller(_assemblies.ToArray(), _removeHubs.ToArray(), _removeModules.ToArray()));
	    }
    }
}