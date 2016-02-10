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

	    internal bool FirstLineIsHeader { get; private set; }
	    internal Encoding Encoding { get; private set; }
	    internal string Delimiter { get; private set; }
    }
}