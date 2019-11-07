using System;
using System.IO;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities.Testing;

namespace Vertica.Integration.Tests.Infrastructure.Archiving
{
	[TestFixture(Category = "Integration,Slow")]
	public class FileBasedArchiveServiceTester
	{
		[Test]
		public void Create_Archives_Delete_Expired()
		{
			string baseDirectory = Guid.NewGuid().ToString("N");

			try
			{
				var logger = Substitute.For<ILogger>();

				var subject = new FileBasedArchiveService(new InMemoryRuntimeSettings()
					.Set(FileBasedArchiveService.BaseDirectoryKey, baseDirectory), logger);

				using (TimeReseter.SetUtc(new DateTimeOffset(2015, 9, 22, 12, 0, 0, TimeSpan.Zero)))
				{
					subject.ArchiveText("Name", "Content", options => options
						.Named("Archive Name 1")
						.ExpiresAfterDays(1));

					ArchiveCreated archive2 = subject.ArchiveText("Name", "Content", options => options
						.Named("Archive Name 2")
						.ExpiresOn(new DateTimeOffset(2015, 10, 1, 12, 0, 0, TimeSpan.Zero)));

					Archive[] archives = subject.GetAll();
					Assert.That(archives.Length, Is.EqualTo(2));

					using (TimeReseter.SetUtc(new DateTimeOffset(2015, 9, 24, 0, 0, 0, TimeSpan.Zero)))
					{
						int expired = subject.DeleteExpired();

						Assert.That(expired, Is.EqualTo(1));

						archives = subject.GetAll();

						Assert.That(archives.Length, Is.EqualTo(1));
						Assert.That(archives[0].Id, Is.EqualTo(archive2.Id));
						Assert.That(archives[0].Name, Is.EqualTo("Archive Name 2"));

						// zip + meta file
						Assert.That(Directory.EnumerateFiles(baseDirectory).Count(), Is.EqualTo(2));
					}
				}
			}
			finally
			{
				Directory.Delete(baseDirectory, true);
			}
		}
	}
}