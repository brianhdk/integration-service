using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public IEnumerable<CsvRow> Parse(Stream stream, bool firstLineIsHeader, Action<CsvConfiguration> csv = null)
		{
	        if (stream == null) throw new ArgumentNullException("stream");

            string delimiter = CsvConfiguration.DefaultDelimiter;

            string[][] lines = _csvReader.Read(stream, configuration =>
            {
                if (csv != null)
                    csv(configuration);

                delimiter = configuration.Delimiter;

            }).ToArray();

	        Dictionary<string, int> headers = null;

	        if (firstLineIsHeader && lines.Length > 0)
	        {
	            headers = lines.First()
	                .Select((name, index) => new { name, index })
	                .ToDictionary(x => x.name, x => x.index, StringComparer.OrdinalIgnoreCase);
	        }

            int lineNumberOffset = headers != null ? 2 : 1;

            return lines
                .Skip(headers != null ? 1 : 0)
                .Select((x, i) => new CsvRow(x, delimiter, headers, (uint?) (i + lineNumberOffset)));
		}
	}
}