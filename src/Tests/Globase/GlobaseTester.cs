using System;
using System.IO;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Globase;
using Vertica.Integration.Globase.Csv;
using Vertica.Integration.Globase.Ftp;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Remote;
using Vertica.Integration.Infrastructure.Remote.Ftp;

namespace Vertica.Integration.Tests.Globase
{
    [TestFixture]
    public class GlobaseTester
    {
        [Test]
        public void Write_Csv_VerifyText()
        {
            using (IApplicationContext context = ApplicationContext.Create(application => application
                .UseGlobase()))
            {
                var records = Enumerable.Range(1, 5).Select(x => new CsvCustomRecord(x));

                var writer = context.Resolve<ICsvWriter>();
                string csv = writer.Write(records, record => { });

                Assert.That(csv, Is.EqualTo(string.Join(Environment.NewLine,
"External id\tProduct Id",
"ExternalId_1\tProductId_1",
"ExternalId_2\tProductId_2",
"ExternalId_3\tProductId_3",
"ExternalId_4\tProductId_4",
"ExternalId_5\tProductId_5")));
            }
        }

        [Test]
        public void Upload_Throws_Exception()
        {
            var archiveService = Substitute.For<IArchiveService>();

            archiveService
                .Create(Arg.Is("TestName"), Arg.Any<Action<ArchiveCreated>>())
                .Returns(new BeginArchive("TestName", (m, o) => { }));

            var ftpClientFactory = Substitute.For<IFtpClientFactory>();
            var ftpClient = Substitute.For<IFtpClient>();

            ftpClientFactory.Create(Arg.Is("ftp://localhost"))
                .Returns(ftpClient);

            ftpClient.Upload(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Is(false))
                .Returns("done");

            using (IApplicationContext context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb.Disable()))
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register(kernel => archiveService)
                        .Register(kernel => ftpClientFactory)))
                .UseGlobase(globase => globase
                    .FtpConnectionString(ConnectionString.FromText("ftp://localhost"))
                    .EnableArchiving(options => options.Named("TestName").ExpiresAfterDays(1)))))
            {
                var file = FtpFile.Csv("records", "content");

                var uploader = context.Resolve<IFtpUploader>();
                string[] result = uploader.Upload(new[] {file});

                CollectionAssert.AreEqual(new[] { "done" }, result);
            }
        }

        public class CsvCustomRecord : CsvRecord<CsvCustomRecord>
        {
            public CsvCustomRecord(int index)
            {
                ExternalId = $"ExternalId_{index}";
                ProductId = $"ProductId_{index}";
            }

            [CsvField("External id")]
            public string ExternalId { get; set; }

            [CsvField("Product Id")]
            public string ProductId { get; set; }
        }
    }
}