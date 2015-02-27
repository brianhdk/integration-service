using System;
using System.IO;
using System.IO.Compression;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public class Archive : IDisposable
    {
        private readonly MemoryStream _stream;
        private readonly ZipArchive _archive;

        public Archive()
        {
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
        }

        public void IncludeFile(FileInfo file)
        {
            _archive.CreateEntryFromFile(file.FullName, file.Name);
        }

        public void Dispose()
        {
            // test only
            using (var fileStream = new FileStream(@"c:\tmp\" + Guid.NewGuid().ToString("N") + ".zip", FileMode.CreateNew))
            {
                _stream.WriteTo(fileStream);
            }

            _archive.Dispose();
            _stream.Dispose();
        }
    }
}