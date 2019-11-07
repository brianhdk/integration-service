using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Tests.Infrastructure.Testing;

namespace Vertica.Integration.Tests.Infrastructure.Factories
{
	[TestFixture]
	public class CollectionInstallerTester
	{
		[Test]
		public void Resolve_As_DifferentDataStructures()
		{
		    using (IApplicationContext context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
		        .Services(services => services
		            .Advanced(advanced => advanced
		                .Install(Install
		                    .Collection<ISomeService>()
		                    .AddFromAssemblyOfThis<CollectionInstallerTester>())))))
		    {
                ISomeService[] enumerable = context.Resolve<IEnumerable<ISomeService>>().ToArray();

                Assert.IsNotNull(enumerable.SingleOrDefault(x => x.GetType() == typeof(SomeServiceImpl)));
                Assert.IsNotNull(enumerable.SingleOrDefault(x => x.GetType() == typeof(SomeOtherServiceImpl)));

                ISomeService[] array = context.Resolve<ISomeService[]>();

                Assert.IsNotNull(array.SingleOrDefault(x => x.GetType() == typeof(SomeServiceImpl)));
                Assert.IsNotNull(array.SingleOrDefault(x => x.GetType() == typeof(SomeOtherServiceImpl)));

                ISomeService[] allArray = context.ResolveAll<ISomeService>();

                Assert.IsNotNull(allArray.SingleOrDefault(x => x.GetType() == typeof(SomeServiceImpl)));
                Assert.IsNotNull(allArray.SingleOrDefault(x => x.GetType() == typeof(SomeOtherServiceImpl)));
            }
		}

		[Test]
		public void Ignore_Service_Gets_Ignored()
		{
		    using (IApplicationContext context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
		        .Services(services => services
		            .Advanced(advanced => advanced
		                .Install(Install
		                    .Collection<ISomeService>()
		                    .AddFromAssemblyOfThis<CollectionInstallerTester>()
                            .Ignore<SomeServiceImpl>())))))
		    {
                ISomeService[] implementations = context.Resolve<ISomeService[]>();

                Assert.IsNull(implementations.SingleOrDefault(x => x.GetType() == typeof(SomeServiceImpl)));
                Assert.IsNotNull(implementations.SingleOrDefault(x => x.GetType() == typeof(SomeOtherServiceImpl)));
            }
        }

		public interface ISomeService
		{
		}

		public class SomeServiceImpl : ISomeService
		{
		}

		public class SomeOtherServiceImpl : ISomeService
		{
		}
	}
}