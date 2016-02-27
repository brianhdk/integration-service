namespace Vertica.Integration.Hangfire
{
	public class ShutdownActions : EventActions<ShutdownActions>
	{
		protected override ShutdownActions This => this;
	}
}