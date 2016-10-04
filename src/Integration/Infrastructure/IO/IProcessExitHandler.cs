namespace Vertica.Integration.Infrastructure.IO
{
	public interface IProcessExitHandler
	{
        //CancellationToken CancellationToken { get; }

		void Wait();
	}
}