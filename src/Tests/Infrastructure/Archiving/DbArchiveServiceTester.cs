using System;
using System.Data;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Database;

namespace Vertica.Integration.Tests.Infrastructure.Archiving
{
	[TestFixture]
    public class DbArchiveServiceTester
    {
        [Test]
        public void ArchiveText_Verify_DbInteraction()
        {
            IDbSession session;
            DbArchiveService subject = Initialize(out session);

            const int expectedId = 1;
            session.ExecuteScalar<int>(null).ReturnsForAnyArgs(expectedId);

            ArchiveCreated archive = subject.ArchiveText("C", new string('C', 10000));

            Assert.That(archive.Id, Is.EqualTo(expectedId.ToString()));
        }

        private DbArchiveService Initialize(out IDbSession session)
        {
            var dbFactory = Substitute.For<IDbFactory>();
            session = Substitute.For<IDbSession>();

            dbFactory.OpenSession().Returns(session);

            IDbTransaction transaction = Substitute.For<IDbTransaction>();
            session.BeginTransaction().Returns(transaction);

            var configuration = Substitute.For<IIntegrationDatabaseConfiguration>();
            var settings = Substitute.For<IRuntimeSettings>();

            return new DbArchiveService(dbFactory, configuration, settings);
        }
    }
}