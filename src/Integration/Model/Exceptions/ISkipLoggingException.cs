namespace Vertica.Integration.Model.Exceptions
{
	/// <summary>
	/// Marker interface to implement on any custom exception which will instruct Integration Service not to log it.
	/// </summary>
	public interface ISkipLoggingException
	{
	}
}