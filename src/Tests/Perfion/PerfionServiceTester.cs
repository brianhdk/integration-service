using System;
using Castle.MicroKernel;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Perfion;
using Vertica.Integration.Perfion.PerfionAPIService;

namespace Vertica.Integration.Tests.Perfion
{
    [TestFixture]
    public class PerfionServiceTester
    {
        private readonly ExecuteQueryResponse _executeQueryResponse = 
            new ExecuteQueryResponse { Body = new ExecuteQueryResponseBody { ExecuteQueryResult = "<result/>" } };

        private IArchiveService _archiveService;
        private IKernel _kernel;
        private IPerfionConfiguration _configuration;
        private GetDataSoap _proxy;

        [Test]
        public void Query_ArchivingEnabledGloballyNotSpecifiedOnQuery_Archives()
        {
            PerfionService subject = CreateSubject(new ArchiveOptions("Global"));
            _proxy.ExecuteQuery(Arg.Any<ExecuteQueryRequest>()).Returns(_executeQueryResponse);

            subject.Query("<query/>");

            _archiveService.Received().Create(Arg.Is("Global"), Arg.Any<Action<ArchiveCreated>>());
        }

        [Test]
        public void Query_ArchivingDisabledGloballyNotSpecifiedOnQuery_DoesNotArchives()
        {
            PerfionService subject = CreateSubject();
            _proxy.ExecuteQuery(Arg.Any<ExecuteQueryRequest>()).Returns(_executeQueryResponse);

            subject.Query("<query/>");

            _archiveService.DidNotReceive().Create(Arg.Any<string>(), Arg.Any<Action<ArchiveCreated>>());
        }

        [Test]
        public void Query_ArchivingEnabledGloballyEnabledOnQuery_ArchivesLocal()
        {
            PerfionService subject = CreateSubject(new ArchiveOptions("Global"));
            _proxy.ExecuteQuery(Arg.Any<ExecuteQueryRequest>()).Returns(_executeQueryResponse);

            subject.Query("<query/>", options => options.Named("Local"));

            _archiveService.Received().Create(Arg.Is("Local"), Arg.Any<Action<ArchiveCreated>>());
        }

        [Test]
        public void Query_ArchivingEnabledGloballyDisabledOnQuery_DoesNotArchive()
        {
            PerfionService subject = CreateSubject(new ArchiveOptions("Global"));
            _proxy.ExecuteQuery(Arg.Any<ExecuteQueryRequest>()).Returns(_executeQueryResponse);

            subject.Query("<query/>", options => options.Disable());

            _archiveService.DidNotReceive().Create(Arg.Any<string>(), Arg.Any<Action<ArchiveCreated>>());
        }

        [Test]
        public void Query_ArchivingDisabledGloballyDisabledOnQuery_DoesNotArchives()
        {
            PerfionService subject = CreateSubject();
            _proxy.ExecuteQuery(Arg.Any<ExecuteQueryRequest>()).Returns(_executeQueryResponse);

            subject.Query("<query/>", options => options.Disable());

            _archiveService.DidNotReceive().Create(Arg.Any<string>(), Arg.Any<Action<ArchiveCreated>>());
        }


        private PerfionService CreateSubject(ArchiveOptions globalArchiveOptions = null)
        {
            _archiveService = Substitute.For<IArchiveService>();
            _kernel = Substitute.For<IKernel>();
            _proxy = Substitute.For<GetDataSoap>();
            _configuration = new ConfigurationStub(globalArchiveOptions, _proxy);

            var beginArchive = new BeginArchive("name", (options, stream) => { });

            _archiveService.Create(Arg.Any<string>(), Arg.Any<Action<ArchiveCreated>>()).Returns(beginArchive);
            _kernel.Resolve<IPerfionConfiguration>().Returns(_configuration);

            var subject = new PerfionService(_archiveService, _kernel);

            return subject;
        }

        internal class ConfigurationStub : IPerfionConfiguration
        {
            private readonly GetDataSoap _proxy;

            public ConfigurationStub(ArchiveOptions globalArchiveOptions, GetDataSoap proxy)
            {
                _proxy = proxy;
                ArchiveOptions = globalArchiveOptions;
            }

            public ConnectionString ConnectionString => null;
            public ArchiveOptions ArchiveOptions { get; }

            public ServiceClientConfiguration ServiceClientConfiguration => null;
            public WebClientConfiguration WebClientConfiguration => null;

            public T WithProxy<T>(IKernel kernel, Func<GetDataSoap, T> client)
            {
                return client(_proxy);
            }
        }
    }
}