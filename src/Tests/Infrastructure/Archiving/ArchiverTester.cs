using System.Data;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Database.Dapper;

namespace Vertica.Integration.Tests.Infrastructure.Archiving
{
    [TestFixture]
    public class ArchiverTester
    {
        [Test]
        public void ArchiveText_Verify_DapperInteraction()
        {
            IDapperSession session;
            Archiver subject = Initialize(out session);

            const int expectedId = 1;
            session.ExecuteScalar<int>(null).ReturnsForAnyArgs(expectedId);

            ArchiveCreated archive = subject.ArchiveText("C", new string('C', 10000));

            Assert.That(archive.Id, Is.EqualTo(expectedId.ToString()));
        }

        private Archiver Initialize(out IDapperSession session)
        {
            var dapper = Substitute.For<IDapperProvider>();
            session = Substitute.For<IDapperSession>();

            dapper
                .OpenSession()
                .Returns(session);

            IDbTransaction transaction = Substitute.For<IDbTransaction>();
            session.BeginTransaction().Returns(transaction);

            return new Archiver(dapper);
        }
    }
}