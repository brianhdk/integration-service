using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Tests.Infrastructure.Factories
{
	[TestFixture]
	public class CollectionInstallerTester
	{
		[Test]
		public void Resolve_All_As_IEnumerable()
		{
			IWindsorContainer container = CastleWindsor.Initialize(new ApplicationConfiguration()
				.AddCustomInstaller(Install.Collection<ISomeService>()
					.AddFromAssemblyOfThis<CollectionInstallerTester>()));

			ISomeService[] implementations = container.Resolve<IEnumerable<ISomeService>>().ToArray();

			Assert.IsNotNull(implementations.SingleOrDefault(x => x.GetType() == typeof(SomeServiceImpl)));
			Assert.IsNotNull(implementations.SingleOrDefault(x => x.GetType() == typeof(SomeOtherServiceImpl)));
		}

		[Test]
		public void Resolve_All_As_Array()
		{
			IWindsorContainer container = CastleWindsor.Initialize(new ApplicationConfiguration()
				.AddCustomInstaller(Install.Collection<ISomeService>()
					.AddFromAssemblyOfThis<CollectionInstallerTester>()));

			ISomeService[] implementations = container.Resolve<ISomeService[]>();

			Assert.IsNotNull(implementations.SingleOrDefault(x => x.GetType() == typeof(SomeServiceImpl)));
			Assert.IsNotNull(implementations.SingleOrDefault(x => x.GetType() == typeof(SomeOtherServiceImpl)));
		}

		[Test]
		public void Ignore_Service_Gets_Ignored()
		{
			IWindsorContainer container = CastleWindsor.Initialize(new ApplicationConfiguration()
				.AddCustomInstaller(Install.Collection<ISomeService>()
					.AddFromAssemblyOfThis<CollectionInstallerTester>()
					.Ignore<SomeServiceImpl>()));

			ISomeService[] implementations = container.Resolve<ISomeService[]>();

			Assert.IsNull(implementations.SingleOrDefault(x => x.GetType() == typeof(SomeServiceImpl)));
			Assert.IsNotNull(implementations.SingleOrDefault(x => x.GetType() == typeof(SomeOtherServiceImpl)));
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