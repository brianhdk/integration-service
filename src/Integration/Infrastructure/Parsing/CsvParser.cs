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

        public IEnumerable<CsvRow> Parse(Stream stream, bool firstLineIsHeader, Action<CsvConfiguration> builder = null)
		{
	        if (stream == null) throw new ArgumentNullException("stream");

            string delimiter = CsvConfiguration.DefaultDelimiter;

            string[][] lines = _csvReader.Read(stream, configuration =>
            {
                if (builder != null)
                    builder(configuration);

                delimiter = configuration.Delimiter;

            }).ToArray();


	        Dictionary<string, int> headers = null;

	        if (firstLineIsHeader && lines.Length > 0)
	        {
	            headers = lines.First()
	                .Select((name, index) => new { name, index })
	                .ToDictionary(x => x.name, x => x.index, StringComparer.OrdinalIgnoreCase);
	        }

			return	lines.Skip(firstLineIsHeader ? 1 : 0).Select(x => new CsvRow(x, headers, delimiter));
		}
	}
}