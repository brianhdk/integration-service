using NUnit.Framework;
using Vertica.Integration.Infrastructure.Archiving;

namespace Vertica.Integration.Tests.Infrastructure.Archiving
{
    [TestFixture]
    public class ArchiverTester
    {
        [Test]
        public void Archive()
        {
            Archiver subject = Initialize();

            using (Archive archive = subject.New())
            {
            }
        }

        private Archiver Initialize()
        {
            return new Archiver();
        }
    }
}