using System;
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
        }

        public DateTimeOffset Created
        {
            get { return _error.Time.ToUniversalTime(); }
        }

        public override string ToString()
        {
            return String.Join(Environment.NewLine, new[]
            {
                _error.Type,
                _error.Message,
                String.Empty,
                String.Format("User: {0}", _error.User),
                String.Format("Host: {0}", _error.HostName),
                String.Format("Details: /elmah.axd/detail?id={0}", _id)
            });
        }
    }
}