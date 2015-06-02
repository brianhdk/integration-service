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
            AddFromAssemblyOfThis<WebApiConfiguration>();
            // we'll remove TaskController but manually add it later - if we're hosting a specific Task through WebApi.
            Remove<TaskController>();
        }

        /// <summary>
        /// Scans the assembly of the defined <typeparamref name="T"></typeparamref> for public classes inheriting <see cref="ApiController"/>.
        /// <para />
        /// </summary>
        /// <typeparam name="T">The type in which its assembly will be scanned.</typeparam>
        public WebApiConfiguration AddFromAssemblyOfThis<T>()
        {
            _scan.Add(typeof (T).Assembly);

            return this;
        }

        /// <summary>
        /// Adds the specified <typeparamref name="TController"/>.
        /// </summary>
        /// <typeparam name="TController">Specifies the <see cref="ApiController"/> to be added.</typeparam>
        public WebApiConfiguration Add<TController>()
            where TController : ApiController
        {
            _add.Add(typeof(TController));

            return this;
        }

        /// <summary>
        /// Skips the specified <typeparamref name="TController" />.
        /// </summary>
        /// <typeparam name="TController">Specifies the <see cref="ApiController"/> that will be skipped.</typeparam>
        public WebApiConfiguration Remove<TController>()
            where TController : ApiController
        {
            _remove.Add(typeof (TController));

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
            container.Install(new WebApiInstaller(_scan.ToArray(), _add.ToArray(), _remove.ToArray()));
        }
    }
}