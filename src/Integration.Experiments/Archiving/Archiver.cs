using System.IO;
using System.IO.Compression;

namespace Vertica.Integration.Experiments.Archiving
{
    public class Archiver
    {
        public void Archive()
        {
            using (var stream = new MemoryStream())
            using (var archive = new ZipArchive(stream))
            {
            }
        }
    }
}