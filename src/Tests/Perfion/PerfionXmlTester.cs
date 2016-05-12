using System.Collections.Generic;
using System.Xml.Linq;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Perfion;

namespace Vertica.Integration.Tests.Perfion
{
	[TestFixture]
	public class PerfionXmlTester
	{
		[Test]
		public void Features_Caption_Localized()
		{
			PerfionXml xml = Parse(Examples.Example1);

			Dictionary<string, PerfionXml.Feature> features = xml.Features();

			Assert.That(features["Category"].Caption("dan"), Is.EqualTo("Kategori"));
			Assert.That(features["ItemNumber"].Caption("en"), Is.EqualTo("Item Number"));
			Assert.That(features["Product"].Caption(), Is.EqualTo("Produkt"));
			Assert.That(features["Section"].Caption("dummy"), Is.Null);
		}

		private static PerfionXml Parse(string xml)
		{
			var service = Substitute.For<IPerfionService>();
			return new PerfionXml(service, XDocument.Parse(xml));
		}
	}
}