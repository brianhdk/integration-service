using System;
using System.Globalization;
using System.Xml.Linq;

namespace Vertica.Integration.Perfion.Infrastructure
{
	public static class ParsingExtensions
	{
		internal static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

		public static int AsInt32(this XAttribute attribute)
		{
			if (attribute == null) throw new ArgumentNullException("attribute");

			int value;
			if (!Int32.TryParse(attribute.Value, out value))
				throw new ArgumentException(
					String.Format("Attribute '{0}' was expected to be an integer, but value was: '{1}'. {2}",
						attribute.Name,
						attribute.Value,
						attribute.Parent),
					"attribute");

			return value;
		}

		public static DateTime AsDateTime(this XAttribute attribute)
		{
			if (attribute == null) throw new ArgumentNullException("attribute");

			DateTime value;
			if (!DateTime.TryParse(attribute.Value, English, DateTimeStyles.None, out value))
				throw new ArgumentException(
					String.Format("Attribute '{0}' was expected to be a DateTime, but value was: '{1}'. {2}",
						attribute.Name,
						attribute.Value,
						attribute.Parent),
					"attribute");

			return value;
		}

		public static Guid AsGuid(this XElement element)
		{
			if (element == null) throw new ArgumentNullException("element");

			Guid value;
			if (!Guid.TryParse(element.Value, out value))
				throw new ArgumentException(
					String.Format("Value '{0}' was expected to be a Guid. Element: {1}",
						element.Value,
						element),
					"element");

			return value;
		}
	}
}