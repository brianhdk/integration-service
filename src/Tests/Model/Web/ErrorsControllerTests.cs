using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Database.Dapper;
using Vertica.Integration.Infrastructure.Logging;
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
            var dapper = Substitute.For<IDapperProvider>();
            var errorList = new List<ErrorLog> { new ErrorLog(new Exception()) };
            dapper.OpenSession().Query<ErrorLog>(Arg.Any<string>()).Returns(errorList);

            var subject = new ErrorsController(dapper)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            // Act
            HttpResponseMessage result = subject.Get();

            // Assert
            var httpContent = (ObjectContent<IEnumerable<ErrorLog>>)result.Content;
            var page = (IEnumerable<ErrorLog>)httpContent.Value;
            Assert.That(page.Count(), Is.EqualTo(errorList.Count));
        }
    }
}
