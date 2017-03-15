using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic.FileIO;

namespace Vertica.Integration.Infrastructure.Parsing
{
    public class CsvParser : ICsvParser
	{
        public IEnumerable<CsvRow> ParseFromFile(FileInfo file, Action<CsvConfiguration> csv = null)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            using (FileStream stream = file.OpenRead())
            {
                return Parse(stream, csv);
            }
        }

        public IEnumerable<CsvRow> Parse(Stream stream, Action<CsvConfiguration> csv = null)
		{
	        if (stream == null) throw new ArgumentNullException(nameof(stream));

			var configuration = new CsvConfiguration();

            csv?.Invoke(configuration);

            string[][] lines = Read(stream, configuration).ToArray();

	        Dictionary<string, int> headers = null;

	        if (configuration.FirstLineIsHeader && lines.Length > 0)
	        {
	            headers = lines.First()
	                .Select((name, index) => new { name, index })
	                .ToDictionary(x => x.name, x => x.index, StringComparer.OrdinalIgnoreCase);
	        }

            int lineNumberOffset = headers != null ? 2 : 1;

            return lines
                .Skip(headers != null ? 1 : 0)
                .Select((x, i) => new CsvRow(x, configuration.Delimiter, headers, (uint?) (i + lineNumberOffset)));
		}

	    private IEnumerable<string[]> Read(Stream stream, CsvConfiguration configuration)
		{
			string[] previousLine = null;
			int? numberOfColumns = null;

			using (var parser = new TextFieldParser(stream, configuration.Encoding))
			{
				parser.SetDelimiters(configuration.Delimiter);
			    parser.HasFieldsEnclosedInQuotes = configuration.HasFieldsEnclosedInQuotes;

				while (!parser.EndOfData)
				{
					string[] currentLine = parser.ReadFields() ?? new string[0];

					if (!numberOfColumns.HasValue)
						numberOfColumns = currentLine.Length;

					if (previousLine != null)
					{
						// Concat the last column previousLine with text from the first column in current line
						previousLine[previousLine.Length - 1] =
							$"{previousLine[previousLine.Length - 1]}{Environment.NewLine}{currentLine[0]}";

						currentLine = previousLine.Concat(currentLine.Skip(1)).ToArray();
					}

					if (currentLine.Length != numberOfColumns.Value)
					{
						previousLine = currentLine;
					}
					else
					{
						previousLine = null;
						yield return currentLine;
					}
				}
			}
		}
	}
}