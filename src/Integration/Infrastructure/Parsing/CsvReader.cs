using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace Vertica.Integration.Infrastructure.Parsing
{
	public class CsvReader : ICsvReader
	{
		public IEnumerable<string[]> Read(Stream stream, Action<CsvConfiguration> csv = null)
		{
		    if (stream == null) throw new ArgumentNullException("stream");

		    var configuration = new CsvConfiguration();

		    if (csv != null)
		        csv(configuration);

			using (var parser = new TextFieldParser(stream, configuration.Encoding))
			{
				parser.SetDelimiters(configuration.Delimiter);

				while (!parser.EndOfData)
				{
					yield return parser.ReadFields() ?? new string[0];
				}
			}
		}
	}
}