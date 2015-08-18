using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.Windsor;
using Vertica.Integration.WebApi.SignalR.Infrastructure.Castle.Windsor;

namespace Vertica.Integration.WebApi.SignalR
{
    public class SignalRConfiguration : IInitializable<IWindsorContainer>
    {
        private readonly List<Assembly> _assemblies;
		//private readonly List<Type> _pipelineModules;

	    internal SignalRConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException("application");

			Application = application
				.Extensibility(extensibility => extensibility.Register(this));

            _assemblies = new List<Assembly>();
			//_pipelineModules = new List<Type>();
        }

        public ApplicationConfiguration Application { get; private set; }

		public SignalRConfiguration AddFromAssemblyOfThis<T>()
        {
            _assemblies.Add(typeof (T).Assembly);

            return this;
        }

	    void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
	    {
			container.Install(new SignalRInstaller(_assemblies.ToArray()));
	    }
    }
}