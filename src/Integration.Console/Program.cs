using System.IO;
using Vertica.Integration.Experiments;
using Vertica.Integration.Perfion;

namespace Vertica.Integration.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (IApplicationContext context = ApplicationContext.Create(application => application
				.NoDatabase()
				.UsePerfion(perfion => perfion
					.ServiceClient(client => client.Advanced(
						binding: binding =>
						{
						}, 
						clientCredentials: clientCredentials =>
						{

						})))
                .Void()))
			{
				context.Resolve<TextWriter>().WriteLine("Hello!");
                context.Execute(args);
			}
		}
	}
}