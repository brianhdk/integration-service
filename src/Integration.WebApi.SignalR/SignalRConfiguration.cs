using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.Windsor;
using Microsoft.AspNet.SignalR.Hubs;

namespace Vertica.Integration.WebApi.SignalR
{
    public class SignalRConfiguration : IInitializable<IWindsorContainer>
    {
        private readonly List<Assembly> _scan;
		//private readonly List<Type> _pipelineModules;

	    internal SignalRConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException("application");

			Application = application
				.Extensibility(extensibility => extensibility.Register(this));

            _scan = new List<Assembly>();
			//_pipelineModules = new List<Type>();
        }

        public ApplicationConfiguration Application { get; private set; }

		public SignalRConfiguration AddFromAssemblyOfThis<T>()
        {
            _scan.Add(typeof (T).Assembly);

            return this;
        }

		//public SignalRConfiguration AddPipelineModule<T>(T module) where T : HubPipelineModule
		//{
		//	_pipelineModules.Add(typeof (T));

		//	return this;
		//}

	    void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
	    {
			//container.Install(new WebApiInstaller(_scan.ToArray(), _add.ToArray(), _remove.ToArray(), _httpServerConfiguration));
	    }
    }
}