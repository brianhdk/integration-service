using Vertica.Integration.Experiments;

namespace Vertica.Integration.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (IApplicationContext context = ApplicationContext.Create(application => application
				.NoDatabase()
				//.UsePerfion(perfion => perfion
					//.WebClient(client => client.Configure((kernel, webClient) =>
					//{
					//})))
                .Void()))
			{
				//context.Resolve<TextWriter>().WriteLine("Hello!");
                context.Execute(args);

				//var perfion = context.Resolve<IPerfionService>();

				//byte[] bytes = perfion.DownloadPdfReport(new[] {78091}, "Produktblad_DK", "DAN");

				//File.WriteAllBytes(@"c:\tmp\perfion.pdf", bytes);
			}
		}
	}
}