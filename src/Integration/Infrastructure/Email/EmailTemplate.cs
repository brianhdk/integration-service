namespace Vertica.Integration.Infrastructure.Email
{
	public abstract class EmailTemplate
	{
		public abstract string Subject { get; }
		public abstract bool IsHtml { get; }
	    public abstract string GetBody();
	}
}