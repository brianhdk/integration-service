using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Vertica.Integration.Infrastructure.Parsing
{
	public interface ICsvReader
	{
        IEnumerable<string[]> Read(Stream stream, Encoding encoding, string delimiter = ";");
	}
}