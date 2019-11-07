using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.IO;

namespace Vertica.Integration.Tests.Infrastructure.IO
{
	[TestFixture]
	public class AzureWebJobShutdownNotifierTester
	{
		[Test]
		public void Wait_Multiple_Threads()
		{
			DirectoryInfo directory = Directory.CreateDirectory($"{Guid.NewGuid().ToString("N")}");

			try
			{
				var subject = new AzureWebJobShutdownRequest(directory);

				Task t1 = Task.Run(() => subject.Wait());
				Task t2 = Task.Run(() => subject.Wait());

				File.WriteAllText($"{directory.FullName}/{Guid.NewGuid().ToString("N")}.txt", string.Empty);
				
				bool wait = Task.WaitAll(new[] {t1, t2}, TimeSpan.FromSeconds(1));

				Assert.That(wait, Is.True);

				Task t3 = Task.Run(() => subject.Wait());
				wait = Task.WaitAll(new[] { t3 }, TimeSpan.FromSeconds(1));

				Assert.That(wait, Is.True);
			}
			finally
			{
				directory.Delete(true);
			}
		}
	}
}