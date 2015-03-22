using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Database.Dapper;
using Vertica.Integration.Portal.Controllers;
using Vertica.Integration.Portal.Models;

namespace Vertica.Integration.Tests.Model.Web
{
    [TestFixture]
    public class ErrorsControllerTests
    {
        [Test]
        public void Get_ErrorsLogged_ReturnErrors()
        {
            // Arrange
            IDapperProvider dapper = Substitute.For<IDapperProvider>();
            var errorList = new List<ErrorLogModel> { new ErrorLogModel() };
            dapper.OpenSession().Query<ErrorLogModel>(Arg.Any<string>()).Returns(errorList);

            var subject = new ErrorsController(dapper)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            // Act
            HttpResponseMessage result = subject.Get();

            // Assert
            var httpContent = (ObjectContent<IEnumerable<ErrorLogModel>>)result.Content;
            var page = (IEnumerable<ErrorLogModel>)httpContent.Value;
            Assert.That(page.Count(), Is.EqualTo(errorList.Count));
        }
    }
}
