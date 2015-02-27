using System;
using System.Data.SqlClient;
using System.IO;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Database.Dapper;
    
namespace Vertica.Integration.Tests.Infrastructure.Archiving
{
    [TestFixture]
    public class ArchiverTester
    {
        [Test, Ignore]
        public void Archive()
        {
            Archiver subject = Initialize();

            using (Archive archive = subject.Create("Test Archive", Console.WriteLine))
            {
                archive.IncludeFile(new FileInfo(@"Castle.Core.dll"));
                archive.IncludeFolder(new DirectoryInfo(@"C:\Users\bhk\Documents\GitHub\checklist"));
                archive.IncludeContent("A", new string('A', 1000));
                archive.IncludeContent("B", new string('B', 3000));
                archive.IncludeBinary("B.bin", new byte[] { 1, 2, 3 });
                //archive.IncludeObject("sadfsdf");
            }
        }

        private Archiver Initialize()
        {
            var dapper = Substitute.For<IDapperProvider>();

            dapper
                .OpenSession()
                .Returns(c => 
                    new DapperSession(
                        new SqlConnection("Integrated Security=SSPI;Data Source=pj-sql01.vertica.dk;Database=IC_PJ_Integration")));

            return new Archiver(dapper);
        }
    }
}