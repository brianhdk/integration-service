using System;
using System.IO;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public static class ArchiverExtensions
    {
        public static int ArchiveFile(this IArchiver archiver, FileInfo file, string archiveName = null)
        {
            if (archiver == null) throw new ArgumentNullException("archiver");
            if (file == null) throw new ArgumentNullException("file");

            int id = default(int);

            using (Archive archive = archiver.Create(archiveName ?? file.Name, archiveId => id = archiveId))
            {
                archive.IncludeFile(file);
            }

            return id;
        }

        public static int ArchiveFolder(this IArchiver archiver, DirectoryInfo folder, string archiveName = null)
        {
            if (archiver == null) throw new ArgumentNullException("archiver");
            if (folder == null) throw new ArgumentNullException("folder");

            int id = default(int);

            using (Archive archive = archiver.Create(archiveName ?? folder.Name, archiveId => id = archiveId))
            {
                archive.IncludeFolder(folder);
            }

            return id;
        }

        public static int ArchiveText(this IArchiver archiver, string name, string content, string archiveName = null)
        {
            if (archiver == null) throw new ArgumentNullException("archiver");

            int id = default(int);

            using (Archive archive = archiver.Create(archiveName ?? name, archiveId => id = archiveId))
            {
                archive.IncludeContent(name, content);
            }

            return id;
        }

        public static int ArchiveObjectAsJson(this IArchiver archiver, object obj, string name, string archiveName = null)
        {
            if (archiver == null) throw new ArgumentNullException("archiver");
            if (obj == null) throw new ArgumentNullException("obj");

            int id = default(int);

            using (Archive archive = archiver.Create(archiveName ?? name, archiveId => id = archiveId))
            {
                archive.IncludeObjectAsJson(obj, name);
            }

            return id;
        }

        public static int Archive(this IArchiver archiver, string name, Action<Archive> archive)
        {
            if (archive == null) throw new ArgumentNullException("archive");

            int id = default(int);

            using (Archive local = archiver.Create(name, archiveId => id = archiveId))
            {
                archive(local);
            }

            return id;            
        }
    }
}