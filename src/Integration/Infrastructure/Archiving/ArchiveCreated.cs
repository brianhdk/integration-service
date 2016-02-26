using System;
using System.Linq;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public class ArchiveCreated
    {
        private readonly string[] _additionalDownloadOptions;

        public ArchiveCreated(string id, params string[] additionalDownloadOptions)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(id));

            Id = id;

            _additionalDownloadOptions = additionalDownloadOptions ?? new string[0];
        }

        public string Id { get; private set; }

        public override string ToString()
        {
            return
	            $"ArchiveID: {Id}\r\nOptions to download archive:\r\n{string.Join(Environment.NewLine, new[] {"From the web-based interface (Portal)", $"Run the following command: {Task.NameOf<DumpArchiveTask>()} {Id}"}.Concat(_additionalDownloadOptions).Select(x => $" - {x}"))}";
        }

        public static implicit operator string(ArchiveCreated archive)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));

            return archive.ToString();
        }
    }
}