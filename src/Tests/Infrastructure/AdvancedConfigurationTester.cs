﻿using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Tests.Infrastructure
{
	[TestFixture]
	public class AdvancedConfigurationTester
	{
		[Test]
		public void Replace_Services()
		{
			using (var context = TestableApplicationContext.Create(application => application
				.Hosts(hosts => hosts.Clear().Host<TestHost<ConfigurationRepositoryStub>>())
				.Advanced(advanced => advanced
					.Register<IConfigurationRepository, ConfigurationRepositoryStub>()
					.Register<IArchiveService, ArchiveServiceStub>()
					.Register<TextWriter, TextWriterStub>())))
			{
				context.Execute();
			}
		}

		[Test]
		public void Replace_Services_With_Specific_Disabled_Service()
		{
			using (var context = TestableApplicationContext.Create(application => application
				.Hosts(hosts => hosts.Clear().Host<TestHost<DisabledConfigurationRepositoryStub>>())
				.Advanced(advanced => advanced
					.Register<IConfigurationRepository, ConfigurationRepositoryStub, DisabledConfigurationRepositoryStub>()
					.Register<IArchiveService, ArchiveServiceStub>()
					.Register<TextWriter, TextWriterStub>())))
			{
				context.Execute();
			}
		}

		[Test]
		public void Replace_Service_With_Instance()
		{
			using (var context = TestableApplicationContext.Create(application => application
				.Hosts(hosts => hosts.Clear().Host<TestHost<InstanceConfigurationRepositoryStub>>())
				.Advanced(advanced => advanced
					.Register<IConfigurationRepository>(InstanceConfigurationRepositoryStub.Create)
					.Register<IArchiveService, ArchiveServiceStub>()
					.Register<TextWriter, TextWriterStub>())))
			{
				context.Execute();
			}			
		}

		public class InstanceConfigurationRepositoryStub : ConfigurationRepositoryStub
		{
			private InstanceConfigurationRepositoryStub()
			{
			}

			public static InstanceConfigurationRepositoryStub Create()
			{
				return new InstanceConfigurationRepositoryStub();
			}
		}

		public class DisabledConfigurationRepositoryStub : ConfigurationRepositoryStub
		{
		}

		public class TestHost<TExpectedConfigurationRepositoryStub> : IHost
			where TExpectedConfigurationRepositoryStub : IConfigurationRepository
		{
			private readonly TextWriter _textWriter;
			private readonly IArchiveService _archiveService;
			private readonly IConfigurationRepository _configurationRepository;

			public TestHost(TextWriter textWriter, IArchiveService archiveService, IConfigurationRepository configurationRepository)
			{
				_textWriter = textWriter;
				_archiveService = archiveService;
				_configurationRepository = configurationRepository;
			}

			public bool CanHandle(HostArguments args)
			{
				return true;
			}

			public void Handle(HostArguments args)
			{
				Assert.That(_textWriter, Is.InstanceOf<TextWriterStub>());
				Assert.That(_archiveService, Is.InstanceOf<ArchiveServiceStub>());
				Assert.That(_configurationRepository, Is.InstanceOf<TExpectedConfigurationRepositoryStub>());
			}

			public string Description
			{
				get { return "TBD"; }
			}
		}

		public class TextWriterStub : TextWriter
		{
			public override Encoding Encoding
			{
				get { return Encoding.Default; }
			}
		}

		public class ArchiveServiceStub : IArchiveService
		{
			public BeginArchive Create(string name, Action<ArchiveCreated> onCreated = null)
			{
				throw new NotSupportedException();
			}

			public Archive[] GetAll()
			{
				throw new NotSupportedException();
			}

			public byte[] Get(string id)
			{
				throw new NotSupportedException();
			}

			public int Delete(DateTimeOffset olderThan)
			{
				throw new NotSupportedException();
			}

			public int DeleteExpired()
			{
				throw new NotSupportedException();
			}
		}

		public class ConfigurationRepositoryStub : IConfigurationRepository
		{
			public Integration.Infrastructure.Configuration.Configuration[] GetAll()
			{
				throw new NotSupportedException();
			}

			public Integration.Infrastructure.Configuration.Configuration Get(string id)
			{
				throw new NotSupportedException();
			}

			public Integration.Infrastructure.Configuration.Configuration Save(Integration.Infrastructure.Configuration.Configuration configuration)
			{
				throw new NotSupportedException();
			}

			public void Delete(string id)
			{
				throw new NotSupportedException();
			}
		}
	}
}