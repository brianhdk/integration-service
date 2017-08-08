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
			if (attribute == null) throw new ArgumentNullException(nameof(attribute));

			int value;
			if (!Int32.TryParse(attribute.Value, out value))
				throw new ArgumentException($@"Attribute '{attribute.Name}' was expected to be an integer, but value was: '{attribute.Value}'. {attribute.Parent}", nameof(attribute));

			return value;
		}

		public static DateTime AsDateTime(this XAttribute attribute)
		{
			if (attribute == null) throw new ArgumentNullException(nameof(attribute));

			DateTime value;
			if (!DateTime.TryParse(attribute.Value, English, DateTimeStyles.None, out value))
				throw new ArgumentException($@"Attribute '{attribute.Name}' was expected to be a DateTime, but value was: '{attribute.Value}'. {attribute.Parent}", nameof(attribute));

			return value;
		}

		public static Guid AsGuid(this XElement element)
		{
			if (element == null) throw new ArgumentNullException(nameof(element));

			Guid value;
			if (!Guid.TryParse(element.Value, out value))
				throw new ArgumentException($@"Value '{element.Value}' was expected to be a Guid. Element: {element}", nameof(element));

			return value;
		}
	}
}