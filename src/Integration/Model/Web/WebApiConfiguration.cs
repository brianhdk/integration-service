using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Model.Web
{
    public class WebApiConfiguration : IInitializable<IWindsorContainer>
    {
        private readonly List<Assembly> _scan;
        private readonly List<Type> _add;
        private readonly List<Type> _remove;

        internal WebApiConfiguration()
        {
            _scan = new List<Assembly>();
            _add = new List<Type>();
            _remove = new List<Type>();

            // scan own assembly
            Scan(GetType().Assembly);
            // we'll remove TaskController but manually add it later - if we're hosting a specific Task through WebApi.
            Remove<TaskController>();
        }

        public WebApiConfiguration Scan(Assembly assemblyToScan)
        {
            if (assemblyToScan == null) throw new ArgumentNullException("assemblyToScan");

            _scan.Add(assemblyToScan);

            return this;
        }

        public WebApiConfiguration Add<T>()
            where T : ApiController
        {
            _add.Add(typeof (T));

            return this;
        }

        public WebApiConfiguration Remove<T>()
            where T : ApiController
        {
            _remove.Add(typeof (T));

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
            container.Install(new WebApiInstaller(_scan.ToArray(), _add.ToArray(), _remove.ToArray()));
        }
    }
}