using System;
using System.Collections.Generic;
using System.IO;

namespace Vertica.Integration.Infrastructure.Parsing
{
	public interface ICsvParser
	{
        IEnumerable<CsvRow> Parse(Stream stream, Action<CsvConfiguration> csv = null);
	}
}