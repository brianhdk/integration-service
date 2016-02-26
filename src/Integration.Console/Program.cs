using Vertica.Integration.Experiments;

namespace Vertica.Integration.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (IApplicationContext context = ApplicationContext.Create(application => application
				.NoDb()
                .Void()))
			{
                context.Execute(args);
			}
		}
	}
}