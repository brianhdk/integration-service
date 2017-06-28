using Castle.MicroKernel;
using NUnit.Framework;
using Vertica.Integration.Tests.Infrastructure.Testing;

namespace Vertica.Integration.Tests.Infrastructure
{
	[TestFixture]
	public class ServicesConventionsConfigurationTester
	{
		[Test]
		public void Following_NamingConvention_CanBeResolved()
		{
            using (var context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .Services(services => services
                    .Conventions(conventions => conventions
                        .AddFromAssemblyOfThis<ServicesConventionsConfigurationTester>()))))
            {
                var someService = context.Resolve<ISomeService>();

                Assert.That(someService, Is.TypeOf<SomeService>());
            }
		}

        [Test]
        public void NotFollowing_NamingConvention_Throws()
        {
            using (var context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .Services(services => services
                    .Conventions(conventions => conventions
                        .AddFromAssemblyOfThis<ServicesConventionsConfigurationTester>()))))
            {
                Assert.Throws<ComponentNotFoundException>(() => context.Resolve<ISomeOtherService>());
            }
        }

        [Test]
        public void Following_NamingConvention_ButIgnored_Throws()
        {
            using (var context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .Services(services => services
                    .Conventions(conventions => conventions
                        .AddFromAssemblyOfThis<ServicesConventionsConfigurationTester>()
                        .Ignore<ISomeService>()))))
            {
                Assert.Throws<ComponentNotFoundException>(() => context.Resolve<ISomeService>());
            }
        }

        public interface ISomeService
	    {
	    }

	    public class SomeService : ISomeService
	    {
	    }

	    public interface ISomeOtherService
	    {
	    }

	    public class SomeOtherServiceImpl : ISomeOtherService
	    {
	        // not following naming convention - thus should not be resolvable
	    }
	}
}