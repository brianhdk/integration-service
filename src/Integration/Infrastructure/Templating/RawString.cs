using System.Net;
using System.Web;

namespace Vertica.Integration.Infrastructure.Templating
{
	public class RawString : IHtmlString
	{
		private readonly string _text;

		public RawString(string text)
		{
			_text = text;
		}

		public override string ToString()
		{
			return _text;
		}

		public string ToHtmlString()
		{
			return WebUtility.HtmlEncode(_text);
		}
	}
}