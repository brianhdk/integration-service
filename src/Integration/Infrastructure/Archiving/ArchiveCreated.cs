using System;
using System.Linq;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public class ArchiveCreated
    {
        private readonly string[] _additionalDownloadOptions;

        public ArchiveCreated(string id, params string[] additionalDownloadOptions)
        {
            if (String.IsNullOrWhiteSpace(id)) throw new ArgumentException(@"Value cannot be null or empty.", "id");

            Id = id;

            _additionalDownloadOptions = additionalDownloadOptions ?? new string[0];
        }

        public string Id { get; private set; }

        public override string ToString()
        {
            return
                String.Format("ArchiveID: {0}\r\nOptions to download archive:\r\n{1}",
                    Id,
                    String.Join(Environment.NewLine, new[]
                    {
                        "From the web-based interface (Portal)",
                        String.Format("Run the following command: {0} {1}", typeof (DumpArchiveTask).Name, Id),
                    }.Concat(_additionalDownloadOptions).Select(x => String.Format(" - {0}", x))));
        }

        public static implicit operator string(ArchiveCreated archive)
        {
            if (archive == null) throw new ArgumentNullException("archive");

            return archive.ToString();
        }
    }
}