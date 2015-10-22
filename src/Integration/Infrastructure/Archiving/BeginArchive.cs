using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Vertica.Utilities_v4.Extensions.StringExt;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public class BeginArchive : IDisposable
    {
        private readonly Action<MemoryStream, ArchiveOptions> _complete;
        private readonly MemoryStream _stream;
        private readonly ZipArchive _archive;

        public BeginArchive(string name, Action<MemoryStream, ArchiveOptions> complete)
        {
            if (complete == null) throw new ArgumentNullException("complete");

            _complete = complete;

            Options = new ArchiveOptions(name);
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create, leaveOpen: true);
        }

        public ArchiveOptions Options { get; private set; }

		public BeginArchive IncludeFile(FileInfo file)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (!file.Exists) throw new ArgumentException(String.Format(@"File '{0}' does not exist.", file.FullName));

            return CreateEntryFromFile(file);
        }

        public BeginArchive IncludeFolder(DirectoryInfo folder)
        {
            if (folder == null) throw new ArgumentNullException("folder");
            if (!folder.Exists) throw new ArgumentException(String.Format(@"File '{0}' does not exist.", folder.FullName));

            var path = new Stack<string>();
            path.Push(folder.Name);

            return IncludeFolderRecursive(folder, path);
        }

        private BeginArchive IncludeFolderRecursive(DirectoryInfo folder, Stack<string> path)
        {
            foreach (FileInfo file in folder.EnumerateFiles())
                CreateEntryFromFile(file, Path.Combine(path.Reverse().ToArray()));

            foreach (DirectoryInfo subDirectory in folder.EnumerateDirectories())
            {
                path.Push(subDirectory.Name);
                IncludeFolderRecursive(subDirectory, path);
                path.Pop();
            }

	        return this;
        }

        private BeginArchive CreateEntryFromFile(FileInfo file, string relativePath = null)
        {
            _archive.CreateEntryFromFile(file.FullName, Path.Combine(relativePath ?? String.Empty, file.Name));

	        return this;
        }

        public BeginArchive IncludeContent(string name, string content, string extension = null)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");

            name = Regex.Replace(name, @"[^\w\s\.]", String.Empty);
            extension = extension.NullIfEmpty() ?? Path.GetExtension(name).NullIfEmpty() ?? ".txt";
            name = Path.GetFileNameWithoutExtension(name);

            ZipArchiveEntry entry = _archive.CreateEntry(String.Format("{0}{1}", name, extension));

            using (var writer = new StreamWriter(entry.Open()))
                writer.Write(content);

	        return this;
        }

        public BeginArchive IncludeBinary(string fileName, byte[] content)
        {
            if (String.IsNullOrWhiteSpace(fileName)) throw new ArgumentException(@"Value cannot be null or empty.", "fileName");

            ZipArchiveEntry entry = _archive.CreateEntry(fileName);

            using (var writer = new BinaryWriter(entry.Open()))
                writer.Write(content);

	        return this;
        }

        public BeginArchive IncludeObjectAsJson(object obj, string fileNameWithoutExtension = null)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            string content = JsonConvert.SerializeObject(obj, Formatting.Indented);

            IncludeContent(fileNameWithoutExtension ?? obj.GetType().Name, content, ".json");

	        return this;
        }

        public void Dispose()
        {
            _archive.Dispose();

            _stream.Seek(0, SeekOrigin.Begin);
            _complete(_stream, Options);

            _stream.Dispose();
        }
    }
}