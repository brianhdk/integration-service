using System;
using System.Collections.Generic;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Model.Hosting
{
    public class HostsConfiguration : IInitializable<IWindsorContainer>
    {
        private readonly List<Type> _add; 
        private readonly List<Type> _remove;

        internal HostsConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException("application");

			Application = application;

            _add = new List<Type>();
            _remove = new List<Type>();

            Host<TaskHost>();
        }

        public ApplicationConfiguration Application { get; private set; }

        /// <summary>
        /// Adds the specified <typeparamref name="THost" />.
        /// </summary>
        /// <typeparam name="THost">Specifies the <see cref="IHost"/> to be added.</typeparam>
        public HostsConfiguration Host<THost>()
            where THost : IHost
        {
            _add.Add(typeof(THost));

            return this;
        }

        /// <summary>
        /// Skips the specified <typeparamref name="THost" />.
        /// </summary>
        /// <typeparam name="THost">Specifies the <see cref="IHost"/> that will be skipped.</typeparam>
        public HostsConfiguration Remove<THost>()
            where THost : IHost
        {
            _remove.Add(typeof(THost));

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
            container.Install(new HostsInstaller(_add.ToArray(), _remove.ToArray()));
			container.Install(new HostFactoryInstaller());
        }

        /// <summary>
        /// Clears all registred Hosts including the built-in Hosts (TaskHost etc.).
        /// </summary>
        public HostsConfiguration Clear()
        {
            _add.Clear();
            _remove.Clear();

            return this;
        }
    }
}