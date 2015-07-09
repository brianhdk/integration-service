using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities_v4;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Domain.Core
{
    [Guid("FBF783F5-0210-448D-BEB9-FD0E9AD6CABF")]
    [Description("Used by the MaintenanceTask.")]
    public class MaintenanceConfiguration
    {
        public MaintenanceConfiguration()
        {
            CleanUpTaskLogEntriesOlderThan = TimeSpan.FromDays(60);
            CleanUpErrorLogEntriesOlderThan = TimeSpan.FromDays(60);

            ArchiveFolders = new ArchiveFoldersConfiguration();
        }

        public TimeSpan CleanUpTaskLogEntriesOlderThan { get; set; }
        public TimeSpan CleanUpErrorLogEntriesOlderThan { get; set; }

        public ArchiveFoldersConfiguration ArchiveFolders { get; private set; }

        public class ArchiveFoldersConfiguration
        {
            public ArchiveFoldersConfiguration()
            {
                Enabled = true;
                EnsureFolders();
            }

            private void EnsureFolders()
            {
                if (Folders == null)
                    Folders = new Folder[0];
            }

            public Folder this[int index]
            {
                get
                {
                    EnsureFolders();
                    return Folders[index];
                }
            }

            public bool Enabled { get; set; }
            public Folder[] Folders { get; set; }

            public Folder[] GetEnabledFolders()
            {
                EnsureFolders();
                return Folders.Where(x => Enabled && x.Enabled && !String.IsNullOrWhiteSpace(x.Path) && x.Handler != null).ToArray();
            }

            public class Folder
            {
                public Folder()
                {
                    Enabled = true;
                    Target = Target.Service;
                    ArchiveOptions = new ArchiveOptions(typeof (ArchiveFoldersStep).StepName()).GroupedBy("Backup");
                }

                public bool Enabled { get; set; }
                public string Path { get; set; }
                public FolderHandler Handler { get; set; }
                public Target Target { get; set; }
                public ArchiveOptions ArchiveOptions { get; private set; }

                public override string ToString()
                {
                    var sb = new StringBuilder();

                    sb.Append(Path);

                    if (Handler != null)
                        sb.AppendFormat(" (handler = {0})", Handler);

                    return sb.ToString();
                }

                public FileInfo[] GetFiles()
                {
                    return Handler.GetFiles(new DirectoryInfo(Path)).Where(x => x.Exists).ToArray();
                }

                public DirectoryInfo[] GetFolders()
                {
                    return Handler.GetFolders(new DirectoryInfo(Path)).Where(x => x.Exists).ToArray();
                }
            }

            public void Add(Func<Folder, FolderHandlers, FolderHandler> folder)
            {
                if (folder == null) throw new ArgumentNullException("folder");

                var local = new Folder();
                local.Handler = folder(local, new FolderHandlers());

                EnsureFolders();
                Folders = Folders.Append(local).ToArray();
            }

            public void Remove(Folder folder)
            {
                if (folder == null) throw new ArgumentNullException("folder");

                EnsureFolders();
                Folders = Folders.Except(new[] { folder }).ToArray();
            }

            public void Clear()
            {
                Folders = new Folder[0];
            }

            public class FolderHandlers
            {
                public FolderHandler Everything()
                {
                    return new EverythingHandler();
                }

                public FolderHandler FilesOlderThan(TimeSpan timeSpan, string searchPattern = null, bool includeSubDirectories = false)
                {
                    return new FilesOlderThanHandler((uint)timeSpan.TotalSeconds, searchPattern, includeSubDirectories);
                }
            }

            public abstract class FolderHandler
            {
                public abstract IEnumerable<FileInfo> GetFiles(DirectoryInfo path);
                public abstract IEnumerable<DirectoryInfo> GetFolders(DirectoryInfo path);
            }

            public class EverythingHandler : FolderHandler
            {
                public override IEnumerable<FileInfo> GetFiles(DirectoryInfo path)
                {
                    return path.EnumerateFiles();
                }

                public override IEnumerable<DirectoryInfo> GetFolders(DirectoryInfo path)
                {
                    return path.EnumerateDirectories();
                }

                public override string ToString()
                {
                    return "All files and folders.";
                }
            }

            public class FilesOlderThanHandler : FolderHandler
            {
                protected FilesOlderThanHandler()
                {
                }

                internal FilesOlderThanHandler(uint seconds, string searchPattern, bool includeSubDirectories)
                {
                    Seconds = seconds;
                    SearchPattern = searchPattern;
                    IncludeSubDirectories = includeSubDirectories;
                }

                public override IEnumerable<FileInfo> GetFiles(DirectoryInfo path)
                {
                    return path.EnumerateFiles(SearchPattern ?? "*", IncludeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                        .Where(x => Time.UtcNow - x.LastWriteTimeUtc > TimeSpan.FromSeconds(Seconds));
                }

                public override IEnumerable<DirectoryInfo> GetFolders(DirectoryInfo path)
                {
                    yield break;
                }

                public uint Seconds { get; set; }
                public string SearchPattern { get; set; }
                public bool IncludeSubDirectories { get; set; }

                public override string ToString()
                {
                    return String.Format("Files older than {0} second(s){1}{2}.", Seconds,
                        SearchPattern != null ? String.Format(" (Search Pattern = {0}", SearchPattern) : String.Empty,
                        IncludeSubDirectories ? " (All directories)" : String.Empty);
                }
            }
        }

        [Obsolete("Each individual archive now controls their own expiration.")]
        public TimeSpan CleanUpArchivesOlderThan { get; set; }
    }
}