using System;
using System.Collections.Generic;
using System.Text;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Infrastructure.Parsing
{
    public class CsvConfiguration
    {
        private static readonly HashSet<string> AllowedDelimiters = new[]
        {
            "\t"
        }.ToHashSet(StringComparer.OrdinalIgnoreCase);

        public const string DefaultDelimiter = ";";

        public CsvConfiguration()
        {
	        FirstLineIsHeader = true;
            Encoding = Encoding.UTF8;
            Delimiter = DefaultDelimiter;
            HasFieldsEnclosedInQuotes = true;
        }

	    public CsvConfiguration NoHeaders()
	    {
		    FirstLineIsHeader = false;

		    return this;
	    }

        public CsvConfiguration ChangeEncoding(Encoding encoding)
        {
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            Encoding = encoding;

            return this;
        }

        public CsvConfiguration ChangeDelimiter(string delimiter)
        {
            if (string.IsNullOrWhiteSpace(delimiter) && !AllowedDelimiters.Contains(delimiter)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(delimiter));

            Delimiter = delimiter;

            return this;
        }

        public CsvConfiguration ChangeHasFieldsEnclosedInQuotes(bool hasFieldsEnclosedInQuotes)
        {
            HasFieldsEnclosedInQuotes = hasFieldsEnclosedInQuotes;

            return this;
        }

	    internal bool FirstLineIsHeader { get; private set; }
	    internal Encoding Encoding { get; private set; }
	    internal string Delimiter { get; private set; }
	    internal bool? HasFieldsEnclosedInQuotes { get; private set; }
    }
}