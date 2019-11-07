using System;
using System.IO;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Tests.Infrastructure.Configuration
{
	[TestFixture(Category = "Integration,Slow")]
	public class FileBasedConfigurationRepositoryTester
	{
		[Test]
		public void Create_Configurations()
		{
			string baseDirectory = Guid.NewGuid().ToString("N");

			try
			{
				var logger = Substitute.For<ILogger>();

				InMemoryRuntimeSettings settings = new InMemoryRuntimeSettings()
					.Set(FileBasedConfigurationRepository.BaseDirectoryKey, baseDirectory);

				var subject = new FileBasedConfigurationRepository(settings, logger);
				{
					var configuration1 = new Integration.Infrastructure.Configuration.Configuration
					{
						Id = "3CB2320D415647FEBEAD02EC17EEB0FA",
						Name = "Configuration1",
						Description = "Description1",
						JsonData = "{ json }",
						UpdatedBy = "BHK"
					};

					var configuration2 = new Integration.Infrastructure.Configuration.Configuration
					{
						Id = string.Join(", ", typeof(FileBasedConfigurationRepositoryTester).FullName, typeof(FileBasedConfigurationRepositoryTester).Assembly.GetName().Name),
						Name = "Configuration2",
						Description = "Description2",
						JsonData = "{ json2 }",
						UpdatedBy = "BHK"
					};

					subject.Save(configuration1);
					subject.Save(configuration2);

					Integration.Infrastructure.Configuration.Configuration storedConfiguration1 = 
						subject.Get(configuration1.Id);

					Assert.IsNotNull(storedConfiguration1);
					Assert.That(storedConfiguration1.JsonData, Is.EqualTo(configuration1.JsonData));
					Assert.That(storedConfiguration1.UpdatedBy, Is.EqualTo(configuration1.UpdatedBy));

					Integration.Infrastructure.Configuration.Configuration storedConfiguration2 = 
						subject.Get(configuration2.Id);

					Assert.IsNotNull(storedConfiguration1);
					Assert.That(storedConfiguration2.Name, Is.EqualTo(configuration2.Name));
					Assert.That(storedConfiguration2.Description, Is.EqualTo(configuration2.Description));

					Assert.That(subject.GetAll().Length, Is.EqualTo(2));

					subject.Delete(configuration2.Id);

					Integration.Infrastructure.Configuration.Configuration[] configurations = subject.GetAll();

					Assert.That(configurations.Length, Is.EqualTo(1));
					Assert.That(configurations[0].Id, Is.EqualTo(configuration1.Id));
				}
			}
			finally
			{
				Directory.Delete(baseDirectory, true);
			}
		}
	}
}