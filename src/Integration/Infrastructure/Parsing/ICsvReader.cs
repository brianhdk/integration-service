using System;
using System.Collections.Generic;
using System.IO;

namespace Vertica.Integration.Infrastructure.Parsing
{
	public interface ICsvReader
	{
        IEnumerable<string[]> Read(Stream stream, Action<CsvConfiguration> builder = null);
	}
}