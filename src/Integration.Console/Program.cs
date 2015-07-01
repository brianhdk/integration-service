using Vertica.Integration.Experiments;
using Vertica.Integration.Portal;

namespace Vertica.Integration.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (ApplicationContext context = ApplicationContext.Create(application => application
                //.Fast()
                //.TestEventLogger()
                //.TestTextFileLogger()
                .UsePortal()
                .Void()))
			{
                context.Execute(args);
			}
		}
	}
}