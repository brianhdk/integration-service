using System.Collections.Generic;
using System.IO;

namespace Vertica.Integration.Infrastructure.Parsing
{
	public interface ICsvParser
	{
        IEnumerable<CsvRow> Parse(Stream stream, bool firstLineIsHeader);
	}
}