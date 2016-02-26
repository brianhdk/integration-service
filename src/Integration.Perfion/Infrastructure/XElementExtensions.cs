using System;
using System.Xml.Linq;

namespace Vertica.Integration.Perfion.Infrastructure
{
	public static class XElementExtensions
	{
		public static XElement ElementOrEmpty(this XElement current, XName name)
		{
			if (current == null) throw new ArgumentNullException(nameof(current));
			if (name == null) throw new ArgumentNullException(nameof(name));

			return current.Element(name) ?? new XElement(name);
		}

		public static XAttribute AttributeOrEmpty(this XElement current, XName name)
		{
			if (current == null) throw new ArgumentNullException(nameof(current));
			if (name == null) throw new ArgumentNullException(nameof(name));

			return current.Attribute(name) ?? new XAttribute(name, string.Empty);
		}

		public static XAttribute AttributeOrThrow(this XElement current, XName name)
		{
			if (current == null) throw new ArgumentNullException(nameof(current));

			XAttribute attribute = current.Attribute(name);

			if (attribute == null)
				throw new InvalidOperationException(
					$"Element is missing expected attribute '{name}'. '{current}'");

			return attribute;
		}
	}
}