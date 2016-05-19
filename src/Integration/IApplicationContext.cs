using System;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration
{
    public interface IApplicationContext : IDisposable
    {
		/// <summary>
		/// Returns a component instance by the service
		/// </summary>
		/// <param name="service">The service to match</param>
		/// <returns></returns>
		object Resolve(Type service);

		/// <summary>
		/// Resolve all valid components that match this service
		/// </summary>
		/// <param name="service">The service to match</param>
		Array ResolveAll(Type service);

		/// <summary>
		/// Returns a component instance by the service.
		/// </summary>
		/// <typeparam name="T">The Service type</typeparam>
		T Resolve<T>();

		/// <summary>
		/// Resolve all valid components that match this type.
		/// </summary>
		/// <typeparam name="T">The service type</typeparam>
	    T[] ResolveAll<T>();

		/// <summary>
		/// Executes the IHost implementation that is suited for the specified arguments.
		/// </summary>
		/// <param name="args">The arguments to be passed to the IHost implementation</param>
        void Execute(params string[] args);

		/// <summary>
		/// Executes the IHost implementation that is suited for the specified arguments.
		/// </summary>
		/// <param name="args">The arguments to be passed to the IHost implementation</param>
		void Execute(HostArguments args);
    }
}