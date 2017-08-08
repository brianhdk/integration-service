using System.Collections.Generic;
using System.Linq;
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

		[Test]
		public void Features_Unit_Localized()
		{
			PerfionXml xml = Parse(Examples.Example1);

			Dictionary<string, PerfionXml.Feature> features = xml.Features();

			Assert.That(features["Category"].Unit("dan"), Is.EqualTo("UnitDAN"));
			Assert.That(features["Category"].Unit("en"), Is.EqualTo("UnitEN"));
		}

		[Test]
		public void IdsOf_Element_Returns_Ids()
		{
			PerfionXml xml = Parse(Examples.Example1);

			PerfionXml.Component product = xml.Components("Product").FirstOrDefault(x => x.Id == 171458);

			Assert.IsNotNull(product);

			int[] ids = product.IdsOf("ExternalItemtext");

			CollectionAssert.AreEqual(new[] { 308928, 310279, 308742 }, ids);
		}

		private static PerfionXml Parse(string xml)
		{
			var client = Substitute.For<IPerfionClient>();

			return new PerfionXml(client, XDocument.Parse(xml));
		}
	}
}