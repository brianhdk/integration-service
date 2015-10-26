using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Vertica.Integration.Perfion.Infrastructure
{
	public static class PerfionXmlExtensions
	{
		public static int Id(this XElement element)
		{
			if (element == null) throw new ArgumentNullException("element");

			return element.AttributeOrThrow("id").AsInt32();
		}

		public static int? ParentId(this XElement element)
		{
			if (element == null) throw new ArgumentNullException("element");

			int value = element.AttributeOrThrow("parentId").AsInt32();

			return value > 0 ? (int?)value : null;
		}

		public static string Language(this XElement element)
		{
			if (element == null) throw new ArgumentNullException("element");

			return element.AttributeOrThrow("language").Value;
		}

		public static DateTime LastModified(this XElement element)
		{
			if (element == null) throw new ArgumentNullException("element");

			return element.AttributeOrThrow("modifiedDate").AsDateTime();
		}

		public static XElement Element(this XElement element, XName name, string language = null)
		{
			if (element == null) throw new ArgumentNullException("element");
			if (name == null) throw new ArgumentNullException("name");

			return element.Elements(name, language).FirstOrDefault();
		}

		public static IEnumerable<XElement> Elements(this XElement element, XName name, string language = null)
		{
			if (element == null) throw new ArgumentNullException("element");
			if (name == null) throw new ArgumentNullException("name");

			IEnumerable<XElement> elements = element.Elements(name);

			return String.IsNullOrWhiteSpace(language)
				? elements
				: elements.Where(x => String.Equals(x.Language(), language, StringComparison.OrdinalIgnoreCase));
		}
	}
}