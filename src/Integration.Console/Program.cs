using System.IO;
using Vertica.Integration.Experiments;

namespace Vertica.Integration.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (IApplicationContext context = ApplicationContext.Create(application => application
				.NoDatabase()
                .Void()))
			{
				context.Resolve<TextWriter>().WriteLine("Hello!");
                context.Execute(args);
			}
		}
	}
}