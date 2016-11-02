using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
	public class ScanAddRemoveInstaller<T> : IWindsorInstaller
	{
	    private readonly Func<ServiceDescriptor, BasedOnDescriptor> _serviceDescriptor;
	    private readonly Action<ComponentRegistration> _configure;

		private readonly List<Assembly> _scan;
		private readonly List<Type> _add;
		private readonly List<Type> _remove;

		public ScanAddRemoveInstaller(Func<ServiceDescriptor, BasedOnDescriptor> serviceDescriptor = null, Action<ComponentRegistration> configure = null)
		{
		    _serviceDescriptor = serviceDescriptor ?? (x => x.FromInterface(typeof(T)));
		    _configure = configure ?? (x => x.LifestyleSingleton());

			_scan = new List<Assembly>();
			_add = new List<Type>();
			_remove = new List<Type>();
		}

		/// <summary>
		/// Scans the assembly of the defined <typeparamref name="TType"></typeparamref> for public classes inheriting <see cref="TType"/>.
		/// <para />
		/// </summary>
		/// <typeparam name="TType">The type in which its assembly will be scanned.</typeparam>
		public void AddFromAssemblyOfThis<TType>()
		{
			_scan.Add(typeof(TType).Assembly);
		}

		/// <summary>
		/// Adds the specified <typeparamref name="TType"/>.
		/// </summary>
		/// <typeparam name="TType">Specifies the <see cref="TType"/> to be added.</typeparam>
		public void Add<TType>() where TType : T
		{
			_add.Add(typeof(TType));
		}

		/// <summary>
		/// Skips the specified <typeparamref name="TType" />.
		/// </summary>
		/// <typeparam name="TType">Specifies the <see cref="TType"/> that will be skipped.</typeparam>
		public void Remove<TType>() where TType : T
		{
			_remove.Add(typeof(TType));
		}

		/// <summary>
		/// Clears all registred types.
		/// </summary>
		public void Clear()
		{
			_remove.Clear();
			_add.Clear();
			_scan.Clear();
		}

	    void IWindsorInstaller.Install(IWindsorContainer container, IConfigurationStore store)
		{
			foreach (Assembly assembly in _scan.Distinct())
			{
                container.Register(
                    Locate(Classes.FromAssembly(assembly))
                        .Unless(x =>
                        {
                            if (_add.Contains(x) || _remove.Contains(x))
                                return true;
    
                            return false;
                        })
                        .Configure(x => _configure?.Invoke(x)));
			}

		    container.Register(
				Locate(Classes.From(_add.Except(_remove).Distinct()))
					.Configure(x => _configure?.Invoke(x)));
		}

	    private BasedOnDescriptor Locate(FromDescriptor fromDescriptor)
	    {
	        return _serviceDescriptor(fromDescriptor.BasedOn<T>().WithService);
	    }
	}
}