using Vertica.Integration.Experiments;
using Vertica.Integration.Model;
using Vertica.Integration.Portal;
using Vertica.Integration.WebApi;

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