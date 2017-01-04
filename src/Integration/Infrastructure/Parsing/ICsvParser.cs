using System;
using System.Collections.Generic;
using System.IO;

namespace Vertica.Integration.Infrastructure.Parsing
{
	public interface ICsvParser
	{
	    IEnumerable<CsvRow> ParseFromFile(FileInfo file, Action<CsvConfiguration> csv = null);

        IEnumerable<CsvRow> Parse(Stream stream, Action<CsvConfiguration> csv = null);
	}
}