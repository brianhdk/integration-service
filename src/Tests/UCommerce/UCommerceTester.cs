using System;
using System.Data;
using Castle.MicroKernel;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Tests.Infrastructure;
using Vertica.Integration.UCommerce;
using Vertica.Integration.UCommerce.Database;

namespace Vertica.Integration.Tests.UCommerce
{
    [TestFixture]
    public class UCommerceTester
    {
        [Test]
        public void Connection_CustomConnection_CanBeResolved()
        {
            var connection = Substitute.For<IDbConnection>();

            using (IApplicationContext context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .UseUCommerce(uCommerce => uCommerce
                    .Connection(new CustomUCommerceDb(connection)))))
            {
                var uCommerceDb = context.Resolve<IDbFactory<UCommerceDb>>();

                Assert.That(uCommerceDb.GetConnection(), Is.SameAs(connection));
            }
        }

        [Test]
        public void Connection_ConnectionStringFromText_CanBeResolved()
        {
            using (IApplicationContext context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .UseUCommerce(uCommerce => uCommerce
                    .Connection(ConnectionString.FromText(".")))))
            {
                Assert.DoesNotThrow(() => context.Resolve<IDbFactory<UCommerceDb>>());
            }
        }

        [Test]
        public void Connection_DefaultConnection_ThrowsConnectionStringNameNotFound()
        {
            using (IApplicationContext context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .UseUCommerce()))
            {
                var factory = context.Resolve<IDbFactory<UCommerceDb>>();

                ArgumentException exception = Assert.Throws<ArgumentException>(() => factory.GetConnection());

                Assert.That(exception.Message, Does.Contain("No ConnectionString found with name 'uCommerceDb'"));
            }
        }
        
        [Test]
        public void Connection_CustomConnectionStringFromName_ThrowsConnectionStringNameNotFound()
        {
            using (IApplicationContext context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .UseUCommerce(uCommerce => uCommerce
                    .Connection(ConnectionString.FromName("CustomConn")))))
            {
                var factory = context.Resolve<IDbFactory<UCommerceDb>>();

                ArgumentException exception = Assert.Throws<ArgumentException>(() => factory.GetConnection());

                Assert.That(exception.Message, Does.Contain("No ConnectionString found with name 'CustomConn'"));
            }
        }

        internal class CustomUCommerceDb : UCommerceDb
        {
            private readonly IDbConnection _connection;

            public CustomUCommerceDb(IDbConnection connection)
                : base(ConnectionString.FromText("."))
            {
                _connection = connection;
            }

            protected internal override IDbConnection GetConnection(IKernel kernel)
            {
                return _connection;
            }
        }
    }
}