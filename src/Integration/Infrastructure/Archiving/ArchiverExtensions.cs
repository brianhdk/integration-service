using System;
using System.IO;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public static class ArchiverExtensions
    {
        public static string ArchiveFile(this IArchiver archiver, FileInfo file, string archiveName = null)
        {
            if (archiver == null) throw new ArgumentNullException("archiver");
            if (file == null) throw new ArgumentNullException("file");

            string id = null;

            using (BeginArchive archive = archiver.Create(archiveName ?? file.Name, archiveId => id = archiveId))
            {
                archive.IncludeFile(file);
            }

            return id;
        }

        public static string ArchiveFolder(this IArchiver archiver, DirectoryInfo folder, string archiveName = null)
        {
            if (archiver == null) throw new ArgumentNullException("archiver");
            if (folder == null) throw new ArgumentNullException("folder");

            string id = null;

            using (BeginArchive archive = archiver.Create(archiveName ?? folder.Name, archiveId => id = archiveId))
            {
                archive.IncludeFolder(folder);
            }

            return id;
        }

        public static string ArchiveText(this IArchiver archiver, string name, string content, string archiveName = null)
        {
            if (archiver == null) throw new ArgumentNullException("archiver");

            string id = null;

            using (BeginArchive archive = archiver.Create(archiveName ?? name, archiveId => id = archiveId))
            {
                archive.IncludeContent(name, content);
            }

            return id;
        }

        public static string ArchiveObjectAsJson(this IArchiver archiver, object obj, string name, string archiveName = null)
        {
            if (archiver == null) throw new ArgumentNullException("archiver");
            if (obj == null) throw new ArgumentNullException("obj");

            string id = null;

            using (BeginArchive archive = archiver.Create(archiveName ?? name, archiveId => id = archiveId))
            {
                archive.IncludeObjectAsJson(obj, name);
            }

            return id;
        }

        public static string Archive(this IArchiver archiver, string name, Action<BeginArchive> archive)
        {
            if (archive == null) throw new ArgumentNullException("archive");

            string id = null;

            using (BeginArchive local = archiver.Create(name, archiveId => id = archiveId))
            {
                archive(local);
            }

            return id;            
        }
    }
}