using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.UI;
using System.Xml;

namespace Vertica.Integration.Model.Web.Formatters
{
    public class HtmlFormatter : BufferedMediaTypeFormatter
    {
        public HtmlFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
        }

        protected HtmlFormatter(BufferedMediaTypeFormatter formatter)
            : base(formatter)
        {
        }

        public override bool CanReadType(Type type)
        {
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override void WriteToStream(Type type, object value, Stream writeStream, HttpContent content)
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

            using (var writer = new StreamWriter(writeStream))
            using (var html = new Html32TextWriter(writer))
            {
                html.Write(value.ToString());
            }
        }
    }
}