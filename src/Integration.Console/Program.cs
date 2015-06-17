using Vertica.Integration.Experiments;

namespace Vertica.Integration.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (ApplicationContext context = ApplicationContext.Create(application => application
                .TestTextFileLogger()))
			{
			    context.Execute(args);
			}
		}
	}
}