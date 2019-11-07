namespace Vertica.Integration.Model.Exceptions
{
	/// <summary>
	/// Marker interface to put on any custom exception which will instruct Integration Service not to log it.
	/// </summary>
	public interface IIgnoreLogging
	{
	}
}