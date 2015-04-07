using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualBasic.FileIO;

namespace Vertica.Integration.Infrastructure.Parsing
{
	public class CsvReader : ICsvReader
	{
		public IEnumerable<string[]> Read(Stream stream, Encoding encoding, string delimiter = ";")
		{
		    if (stream == null) throw new ArgumentNullException("stream");
		    if (encoding == null) throw new ArgumentNullException("encoding");

			using (var parser = new TextFieldParser(stream, encoding))
			{
				parser.SetDelimiters(delimiter);

				while (!parser.EndOfData)
				{
					yield return parser.ReadFields() ?? new string[0];
				}
			}
		}
	}
}