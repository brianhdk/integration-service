using System;
using System.IO;
using System.IO.Compression;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public class Archive : IDisposable
    {
        private readonly Action<MemoryStream> _complete;

        private readonly MemoryStream _stream;
        private readonly ZipArchive _archive;

        public Archive(Action<MemoryStream> complete)
        {
            if (complete == null) throw new ArgumentNullException("complete");

            _complete = complete;

            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
        }

        public void IncludeFile(FileInfo file)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (!file.Exists) throw new ArgumentException(String.Format(@"File '{0}' does not exist.", file.FullName));

            _archive.CreateEntryFromFile(file.FullName, file.Name);
        }

        public void IncludeFolder(DirectoryInfo folder)
        {
            if (folder == null) throw new ArgumentNullException("folder");
            if (!folder.Exists) throw new ArgumentException(String.Format(@"File '{0}' does not exist.", folder.FullName));

            throw new NotImplementedException();
        }

        public void IncludeObject(object obj)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _stream.Seek(0, SeekOrigin.Begin);
            _complete(_stream);

            _archive.Dispose();
            _stream.Dispose();
        }
    }
}