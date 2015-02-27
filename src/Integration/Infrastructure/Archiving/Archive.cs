using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

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
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create, leaveOpen: true);
        }

        public void IncludeFile(FileInfo file)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (!file.Exists) throw new ArgumentException(String.Format(@"File '{0}' does not exist.", file.FullName));

            CreateEntryFromFile(file);
        }

        public void IncludeFolder(DirectoryInfo folder)
        {
            if (folder == null) throw new ArgumentNullException("folder");
            if (!folder.Exists) throw new ArgumentException(String.Format(@"File '{0}' does not exist.", folder.FullName));

            var path = new Stack<string>();
            path.Push(folder.Name);

            IncludeFolderRecursive(folder, path);
        }

        private void IncludeFolderRecursive(DirectoryInfo folder, Stack<string> path)
        {
            foreach (FileInfo file in folder.EnumerateFiles())
                CreateEntryFromFile(file, Path.Combine(path.Reverse().ToArray()));

            foreach (DirectoryInfo subDirectory in folder.EnumerateDirectories())
            {
                path.Push(subDirectory.Name);
                IncludeFolderRecursive(subDirectory, path);
                path.Pop();
            }
        }

        private void CreateEntryFromFile(FileInfo file, string relativePath = null)
        {
            _archive.CreateEntryFromFile(file.FullName, Path.Combine(relativePath ?? String.Empty, file.Name));
        }

        public void IncludeContent(string name, string content, string extension = "txt")
        {
            name = Regex.Replace(name, @"[^\w\s]", String.Empty);

            ZipArchiveEntry entry = _archive.CreateEntry(String.Format("{0}.{1}", name, extension));

            using (var writer = new StreamWriter(entry.Open()))
                writer.Write(content);
        }

        public void IncludeBinary(string fileName, byte[] content)
        {
            ZipArchiveEntry entry = _archive.CreateEntry(fileName);

            using (var writer = new BinaryWriter(entry.Open()))
                writer.Write(content);               
        }

        public void IncludeObject(object obj)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _archive.Dispose();

            _stream.Seek(0, SeekOrigin.Begin);
            _complete(_stream);

            _stream.Dispose();
        }
    }
}