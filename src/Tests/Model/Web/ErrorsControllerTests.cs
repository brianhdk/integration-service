using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Database.PetaPoco;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model.Web;
using Vertica.Integration.Portal.Controllers;

namespace Vertica.Integration.Tests.Model.Web
{
    [TestFixture]
    public class ErrorsControllerTests
    {
        [Test]
        public void Get_ErrorsLogged_ReturnErrors()
        {
            // Arrange
            var dbFactory = Substitute.For<IDbFactory>();
            var errorList = new List<ErrorLog> { new ErrorLog(new Exception()) };
            dbFactory.OpenDatabase().Page<ErrorLog>(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<string>()).Returns(new Page<ErrorLog> { Items = errorList });

            var subject = new ErrorsController(dbFactory)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            // Act
            HttpResponseMessage result = subject.Get(0, 10);

            // Assert
            var httpContent = (ObjectContent<IPage<ErrorLog>>) result.Content;
            var page = (IPage<ErrorLog>) httpContent.Value;
            Assert.That(page.Items.Count, Is.EqualTo(errorList.Count));
        }
    }
}
