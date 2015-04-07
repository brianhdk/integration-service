using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Vertica.Integration.Infrastructure.Parsing
{
	public class CsvParser : ICsvParser
	{
        private readonly ICsvReader _csvReader;

        public CsvParser(ICsvReader csvReader)
        {
            if (csvReader == null) throw new ArgumentNullException("csvReader");

            _csvReader = csvReader;
        }

	    public IEnumerable<CsvRow> Parse(Stream stream, bool firstLineIsHeader)
		{
	        if (stream == null) throw new ArgumentNullException("stream");

	        string[][] lines = _csvReader.Read(stream, Encoding.UTF8).ToArray();

	        Dictionary<string, int> headers = null;

	        if (firstLineIsHeader && lines.Length > 0)
	        {
	            headers = lines.First()
	                .Select((name, index) => new { name, index })
	                .ToDictionary(x => x.name, x => x.index, StringComparer.OrdinalIgnoreCase);
	        }

			return	lines.Skip(1).Select(x => new CsvRow(x, headers));
		}
	}
}