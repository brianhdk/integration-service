using System;
using System.IO;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public static class ArchiveExtensions
    {
        public static ArchiveCreated ArchiveFile(this IArchiveService service, FileInfo file, string archiveName = null)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (file == null) throw new ArgumentNullException("file");

            ArchiveCreated created = null;

            using (BeginArchive archive = service.Create(archiveName ?? file.Name, x => created = x))
            {
                archive.IncludeFile(file);
            }

            return created;
        }

        public static ArchiveCreated ArchiveFolder(this IArchiveService service, DirectoryInfo folder, string archiveName = null)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (folder == null) throw new ArgumentNullException("folder");

            ArchiveCreated created = null;

            using (BeginArchive archive = service.Create(archiveName ?? folder.Name, x => created = x))
            {
                archive.IncludeFolder(folder);
            }

            return created;
        }

        public static ArchiveCreated ArchiveText(this IArchiveService service, string name, string content, string archiveName = null)
        {
            if (service == null) throw new ArgumentNullException("service");

            ArchiveCreated created = null;

            using (BeginArchive archive = service.Create(archiveName ?? name, x => created = x))
            {
                archive.IncludeContent(name, content);
            }

            return created;
        }

        public static ArchiveCreated ArchiveObjectAsJson(this IArchiveService service, object obj, string name, string archiveName = null)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (obj == null) throw new ArgumentNullException("obj");

            ArchiveCreated created = null;

            using (BeginArchive archive = service.Create(archiveName ?? name, x => created = x))
            {
                archive.IncludeObjectAsJson(obj, name);
            }

            return created;
        }

        public static ArchiveCreated Archive(this IArchiveService service, string name, Action<BeginArchive> archive)
        {
            if (archive == null) throw new ArgumentNullException("service");

            ArchiveCreated created = null;

            using (BeginArchive local = service.Create(name, x => created = x))
            {
                archive(local);
            }

            return created;            
        }
    }
}