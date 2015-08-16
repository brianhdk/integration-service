using System;
using System.Text;

namespace Vertica.Integration.Infrastructure.Parsing
{
    public class CsvConfiguration
    {
        public const string DefaultDelimiter = ";";

        public CsvConfiguration()
        {
	        FirstLineIsHeader = true;
            Encoding = Encoding.UTF8;
            Delimiter = DefaultDelimiter;
        }

	    public CsvConfiguration NoHeaders()
	    {
		    FirstLineIsHeader = false;

		    return this;
	    }

        public CsvConfiguration ChangeEncoding(Encoding encoding)
        {
            if (encoding == null) throw new ArgumentNullException("encoding");

            Encoding = encoding;

            return this;
        }

        public CsvConfiguration ChangeDelimiter(string delimiter)
        {
            if (String.IsNullOrWhiteSpace(delimiter)) throw new ArgumentException(@"Value cannot be null or empty.", "delimiter");

            Delimiter = delimiter;

            return this;
        }

		public bool FirstLineIsHeader { get; private set; }
        public Encoding Encoding { get; private set; }
        public string Delimiter { get; private set; }
    }
}