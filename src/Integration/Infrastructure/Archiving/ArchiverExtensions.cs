using System;
using System.IO;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public static class ArchiverExtensions
    {
        public static ArchiveCreated ArchiveFile(this IArchiver archiver, FileInfo file, string archiveName = null)
        {
            if (archiver == null) throw new ArgumentNullException("archiver");
            if (file == null) throw new ArgumentNullException("file");

            ArchiveCreated created = null;

            using (BeginArchive archive = archiver.Create(archiveName ?? file.Name, x => created = x))
            {
                archive.IncludeFile(file);
            }

            return created;
        }

        public static ArchiveCreated ArchiveFolder(this IArchiver archiver, DirectoryInfo folder, string archiveName = null)
        {
            if (archiver == null) throw new ArgumentNullException("archiver");
            if (folder == null) throw new ArgumentNullException("folder");

            ArchiveCreated created = null;

            using (BeginArchive archive = archiver.Create(archiveName ?? folder.Name, x => created = x))
            {
                archive.IncludeFolder(folder);
            }

            return created;
        }

        public static ArchiveCreated ArchiveText(this IArchiver archiver, string name, string content, string archiveName = null)
        {
            if (archiver == null) throw new ArgumentNullException("archiver");

            ArchiveCreated created = null;

            using (BeginArchive archive = archiver.Create(archiveName ?? name, x => created = x))
            {
                archive.IncludeContent(name, content);
            }

            return created;
        }

        public static ArchiveCreated ArchiveObjectAsJson(this IArchiver archiver, object obj, string name, string archiveName = null)
        {
            if (archiver == null) throw new ArgumentNullException("archiver");
            if (obj == null) throw new ArgumentNullException("obj");

            ArchiveCreated created = null;

            using (BeginArchive archive = archiver.Create(archiveName ?? name, x => created = x))
            {
                archive.IncludeObjectAsJson(obj, name);
            }

            return created;
        }

        public static ArchiveCreated Archive(this IArchiver archiver, string name, Action<BeginArchive> archive)
        {
            if (archive == null) throw new ArgumentNullException("archive");

            ArchiveCreated created = null;

            using (BeginArchive local = archiver.Create(name, x => created = x))
            {
                archive(local);
            }

            return created;            
        }
    }
}