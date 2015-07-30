using NUnit.Framework;
using Vertica.Integration.Infrastructure.Templating;

namespace Vertica.Integration.Tests.Infrastructure.RazorEngine
{
	[TestFixture]
	public class RazorEngineTester
	{
		[TestFixtureSetUp]
		public void Setup()
		{

		}

		[Test]
		public void WriteAttribute()
		{
			var result = InMemoryRazorEngine.Execute(@"<a class=""@String.Format(""test {0}"", 0)""></a>");
			Assert.AreEqual(@"<a class=""test 0""></a>", result);
		}
	}
}
