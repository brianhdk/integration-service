using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model.Web;

namespace Vertica.Integration.Tests.Model.Web
{
    [TestFixture]
    public class ErrorsControllerTests
    {
        [Test]
        public void Get_ErrorsLogged_ReturnErrors()
        {
            // Arrange
            IDbFactory dbFactory = Substitute.For<IDbFactory>();
            var errorList = new List<ErrorLog> { new ErrorLog(new Exception()) };
            dbFactory.OpenDatabase().Query<ErrorLog>(Arg.Any<string>()).Returns(errorList);

            ErrorsController subject = new ErrorsController(dbFactory)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            // Act
            HttpResponseMessage result = subject.Get();

            // Assert
            ObjectContent<IEnumerable<ErrorLog>> httpContent = (ObjectContent<IEnumerable<ErrorLog>>) result.Content;
            IEnumerable<ErrorLog> errors = (IEnumerable<ErrorLog>) httpContent.Value;
            Assert.That(errors.Count(), Is.EqualTo(errorList.Count));
        }
    }
}
