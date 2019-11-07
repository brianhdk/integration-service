using System;
using System.IO;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public static class ArchiveExtensions
    {
        public static ArchiveCreated ArchiveFile(this IArchiveService service, FileInfo file, Action<ArchiveOptions> options = null)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (file == null) throw new ArgumentNullException(nameof(file));

            ArchiveCreated created = null;

            using (BeginArchive archive = service.Create(file.Name, x => created = x))
            {
                options?.Invoke(archive.Options);

                archive.IncludeFile(file);
            }

            return created;
        }

        public static ArchiveCreated ArchiveFolder(this IArchiveService service, DirectoryInfo folder, Action<ArchiveOptions> options = null)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (folder == null) throw new ArgumentNullException(nameof(folder));

            ArchiveCreated created = null;

            using (BeginArchive archive = service.Create(folder.Name, x => created = x))
            {
                options?.Invoke(archive.Options);

                archive.IncludeFolder(folder);
            }

            return created;
        }

        public static ArchiveCreated ArchiveText(this IArchiveService service, string name, string content, Action<ArchiveOptions> options = null)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            ArchiveCreated created = null;

            using (BeginArchive archive = service.Create(name, x => created = x))
            {
                options?.Invoke(archive.Options);

                archive.IncludeContent(name, content);
            }

            return created;
        }

        public static ArchiveCreated ArchiveObjectAsJson(this IArchiveService service, object obj, string name, Action<ArchiveOptions> options = null)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            ArchiveCreated created = null;

            using (BeginArchive archive = service.Create(name, x => created = x))
            {
                options?.Invoke(archive.Options);

                archive.IncludeObjectAsJson(obj, name);
            }

            return created;
        }

        public static ArchiveCreated Archive(this IArchiveService service, string name, Action<BeginArchive> archive)
        {
            if (archive == null) throw new ArgumentNullException(nameof(service));

            ArchiveCreated created = null;

            using (BeginArchive local = service.Create(name, x => created = x))
            {
                archive(local);
            }

            return created;            
        }
    }
}