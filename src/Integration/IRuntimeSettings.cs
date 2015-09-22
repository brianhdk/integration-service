using System.Collections;

namespace Vertica.Integration
{
	/// <summary>
	/// Provides read-only runtime settings such as name of Environment.
	/// </summary>
	public interface IRuntimeSettings
	{
		/// <summary>
		/// Name of the Runtime Environment, e.g. Development, Staging, Production and such.
		/// </summary>
		ApplicationEnvironment Environment { get; }

		/// <summary>
		/// Reads any custom setting based on it's name.
		/// </summary>
		/// <param name="name">Name of the custom setting to be read.</param>
		/// <returns>Value of custom setting.</returns>
		string this[string name] { get; }
	}
}