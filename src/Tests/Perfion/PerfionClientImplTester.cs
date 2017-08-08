using System;
using Castle.MicroKernel;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Perfion;
using Vertica.Integration.Perfion.Infrastructure.Client;
using Vertica.Integration.Perfion.PerfionAPIService;

namespace Vertica.Integration.Tests.Perfion
{
    [TestFixture]
    public class PerfionClientImplTester
    {
        private readonly ExecuteQueryResponse _executeQueryResponse = new ExecuteQueryResponse
        {
            Body = new ExecuteQueryResponseBody
            {
                ExecuteQueryResult = "<result/>"
            }
        };

        private IArchiveService _archiveService;
        private IKernel _kernel;
        private Connection _connection;
        private IPerfionClientConfiguration _configuration;
        private GetDataSoap _proxy;

        [Test]
        public void Query_ArchivingEnabledGloballyNotSpecifiedOnQuery_Archives()
        {
            PerfionClientImpl subject = CreateSubject(new ArchiveOptions("Global"));
            _proxy.ExecuteQuery(Arg.Any<ExecuteQueryRequest>()).Returns(_executeQueryResponse);

            subject.Query("<query/>");

            _archiveService.Received().Create(Arg.Is("Global"), Arg.Any<Action<ArchiveCreated>>());
        }

        [Test]
        public void Query_ArchivingDisabledGloballyNotSpecifiedOnQuery_DoesNotArchives()
        {
            PerfionClientImpl subject = CreateSubject();
            _proxy.ExecuteQuery(Arg.Any<ExecuteQueryRequest>()).Returns(_executeQueryResponse);

            subject.Query("<query/>");

            _archiveService.DidNotReceive().Create(Arg.Any<string>(), Arg.Any<Action<ArchiveCreated>>());
        }

        [Test]
        public void Query_ArchivingEnabledGloballyEnabledOnQuery_ArchivesLocal()
        {
            PerfionClientImpl subject = CreateSubject(new ArchiveOptions("Global"));
            _proxy.ExecuteQuery(Arg.Any<ExecuteQueryRequest>()).Returns(_executeQueryResponse);

            subject.Query("<query/>", options => options.Named("Local"));

            _archiveService.Received().Create(Arg.Is("Local"), Arg.Any<Action<ArchiveCreated>>());
        }

        [Test]
        public void Query_ArchivingEnabledGloballyDisabledOnQuery_DoesNotArchive()
        {
            PerfionClientImpl subject = CreateSubject(new ArchiveOptions("Global"));
            _proxy.ExecuteQuery(Arg.Any<ExecuteQueryRequest>()).Returns(_executeQueryResponse);

            subject.Query("<query/>", options => options.Disable());

            _archiveService.DidNotReceive().Create(Arg.Any<string>(), Arg.Any<Action<ArchiveCreated>>());
        }

        [Test]
        public void Query_ArchivingDisabledGloballyDisabledOnQuery_DoesNotArchives()
        {
            PerfionClientImpl subject = CreateSubject();
            _proxy.ExecuteQuery(Arg.Any<ExecuteQueryRequest>()).Returns(_executeQueryResponse);

            subject.Query("<query/>", options => options.Disable());

            _archiveService.DidNotReceive().Create(Arg.Any<string>(), Arg.Any<Action<ArchiveCreated>>());
        }
        
        private PerfionClientImpl CreateSubject(ArchiveOptions globalArchiveOptions = null)
        {
            _archiveService = Substitute.For<IArchiveService>();
            _kernel = Substitute.For<IKernel>();
            _kernel.Resolve<IArchiveService>().Returns(_archiveService);
            _proxy = Substitute.For<GetDataSoap>();
            _connection = new ConnectionStub(_proxy);
            _configuration = new ConfigurationStub(globalArchiveOptions);

            var beginArchive = new BeginArchive("name", (options, stream) => { });

            _archiveService.Create(Arg.Any<string>(), Arg.Any<Action<ArchiveCreated>>()).Returns(beginArchive);

            var subject = new PerfionClientImpl(_connection, _configuration, _kernel);

            return subject;
        }

        internal class ConfigurationStub : IPerfionClientConfiguration
        {
            public ConfigurationStub(ArchiveOptions globalArchiveOptions)
            {
                ArchiveOptions = globalArchiveOptions;
            }

            public ArchiveOptions ArchiveOptions { get; }
        }

        internal class ConnectionStub : Connection
        {
            private readonly GetDataSoap _proxy;

            public ConnectionStub(GetDataSoap proxy)
                : base(ConnectionString.FromText("http://server/perfion/getdata.asmx"))
            {
                _proxy = proxy;
            }

            internal override T WithProxy<T>(IKernel kernel, Func<GetDataSoap, T> client)
            {
                return client(_proxy);
            }
        }
    }
}