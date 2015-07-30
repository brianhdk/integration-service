using System.Net;

namespace Vertica.Integration.Infrastructure.Templating
{
	public class HtmlHelper
	{
		public RawString Raw(string html)
		{
			return new RawString(html);
		}

		public RawString HtmlString(object value)
		{
			if (value == null)
				return null;

			return new RawString(value.ToString());
		}

		public string Encode(string value)
		{
			if (value == null)
				return string.Empty;
			return WebUtility.HtmlEncode(value);
		}

		public string Encode(object value)
		{
			if (value == null)
				return string.Empty;

			return Encode(value.ToString());
		}
	}
}