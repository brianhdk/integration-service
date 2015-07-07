using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Elmah;

namespace Vertica.Integration.Logging.Elmah
{
    internal class ElmahError
    {
        private readonly string _id;
        private readonly Error _error;

        public ElmahError(string fileName)
        {
            using (var reader = new XmlTextReader(fileName))
            {
                if (!reader.IsStartElement("error"))
                    throw new InvalidOperationException("The error XML is not in the expected format.");

                _id = reader.GetAttribute("errorId");
                _error = ErrorXml.Decode(reader);
            }
        }

        public ElmahError(XmlReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            if (!reader.IsStartElement("error"))
                throw new InvalidOperationException("The error XML is not in the expected format.");

            _id = reader.GetAttribute("errorId");
            _error = ErrorXml.Decode(reader);

            _error.ServerVariables["URL"] = reader.GetAttribute("url");
            _error.ServerVariables["QUERY_STRING"] = reader.GetAttribute("query_string");
            _error.ServerVariables["HTTP_REFERER"] = reader.GetAttribute("http_referer");
        }

        public DateTimeOffset Created
        {
            get { return _error.Time.ToUniversalTime(); }
        }

        public string Source
        {
            get { return _error.Source; }
        }

        public override string ToString()
        {
            return String.Join(Environment.NewLine, new[]
            {
                _error.Type,
                _error.Message,
                String.Empty
            }.Concat(Data.Select(x => 
                String.Format("{0}: {1}", x.Item1, x.Item2))));
        }

        private IEnumerable<Tuple<string, string>> Data
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(_error.User))
                    yield return Tuple.Create("User", _error.User);

                if (!String.IsNullOrWhiteSpace(_error.HostName))
                    yield return Tuple.Create("Host", _error.HostName);

                string url = GetUrl();

                if (!String.IsNullOrWhiteSpace(url))
                    yield return Tuple.Create("URL", url);

                string referer = _error.ServerVariables["HTTP_REFERER"];

                if (!String.IsNullOrWhiteSpace(referer))
                    yield return Tuple.Create("HTTP Referer", referer);

                yield return Tuple.Create("Details", String.Format("/elmah.axd/detail?id={0}", _id));
            }
        }

        private string GetUrl()
        {
            string url = _error.ServerVariables["URL"] ?? String.Empty;
            string queryString = _error.ServerVariables["QUERY_STRING"];

            if (!String.IsNullOrWhiteSpace(queryString))
                return String.Format("{0}?{1}", url, queryString);

            return url;
        }
    }
}