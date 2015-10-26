using System;
using System.Xml.Linq;

namespace Vertica.Integration.Perfion.Infrastructure
{
	public static class XElementExtensions
	{
		public static XElement ElementOrEmpty(this XElement current, XName name)
		{
			if (current == null) throw new ArgumentNullException("current");
			if (name == null) throw new ArgumentNullException("name");

			return current.Element(name) ?? new XElement(name);
		}

		public static XAttribute AttributeOrEmpty(this XElement current, XName name)
		{
			if (current == null) throw new ArgumentNullException("current");
			if (name == null) throw new ArgumentNullException("name");

			return current.Attribute(name) ?? new XAttribute(name, String.Empty);
		}

		public static XAttribute AttributeOrThrow(this XElement current, XName name)
		{
			if (current == null) throw new ArgumentNullException("current");

			XAttribute attribute = current.Attribute(name);

			if (attribute == null)
				throw new InvalidOperationException(
					String.Format("Element is missing expected attribute '{0}'. '{1}'", name, current));

			return attribute;
		}
	}
}