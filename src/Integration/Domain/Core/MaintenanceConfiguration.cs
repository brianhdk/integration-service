using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities;
using Vertica.Utilities.Extensions.EnumerableExt;

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

        public ArchiveFoldersConfiguration ArchiveFolders { get; }

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

                return Folders
                    .Where(x => Enabled && x.Enabled && !string.IsNullOrWhiteSpace(x.Path) && x.Handler != null)
                    .ToArray();
            }

            public class Folder
            {
                public Folder()
                {
                    Enabled = true;
                    Target = Target.Service;

                    ArchiveOptions = new PersistedArchiveOptions(nameof(ArchiveFoldersStep));
                    ArchiveOptions
                        .GroupedBy("Backup")
                        .Compression(CompressionLevel.Optimal);
                }

                public string Name { get; set; }
                public bool Enabled { get; set; }
                public string Path { get; set; }
                public FolderHandler Handler { get; set; }
                public Target Target { get; set; }
                public PersistedArchiveOptions ArchiveOptions { get; }

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

                public class PersistedArchiveOptions : ArchiveOptions
                {
                    public PersistedArchiveOptions(string name)
                        : base(name)
                    {
                    }

                    public override ArchiveOptions ExpiresAfter(TimeSpan timeSpan)
                    {
                        ExpirationPeriod = timeSpan;
                        ExpirationMonths = null;

                        return this;
                    }

                    public override ArchiveOptions ExpiresAfterMonths(uint months)
                    {
                        ExpirationMonths = months;
                        ExpirationPeriod = null;

                        return this;
                    }

                    [JsonProperty]
                    public TimeSpan? ExpirationPeriod { get; private set; }

                    [JsonProperty]
                    public uint? ExpirationMonths { get; private set; }

                    [JsonIgnore]
                    public override DateTimeOffset? Expires
                    {
                        get
                        {
                            if (ExpirationPeriod.HasValue)
                                base.ExpiresAfter(ExpirationPeriod.Value);

                            if (ExpirationMonths.HasValue)
                                base.ExpiresAfterMonths(ExpirationMonths.Value);

                            return base.Expires;
                        }
                    }
                }
            }

            [Obsolete("Use the AddOrUpdate() method instead.")]
            public void Add(Func<Folder, FolderHandlers, FolderHandler> folder)
            {
                AddOrUpdate(null, folder);
            }

            /// <summary>
            /// Adds or updates a folder to the archive folder configuration.
            /// </summary>
            /// <param name="name">Friendly name for this Folder. If a folder exists by the name, this folder will be updated.</param>
            /// <param name="folder">Configuration of the folder.</param>
            /// <returns>The actual </returns>
            public Folder AddOrUpdate(string name, Func<Folder, FolderHandlers, FolderHandler> folder)
            {
                if (folder == null) throw new ArgumentNullException(nameof(folder));

                EnsureFolders();

                Folder existing = null;

                if (!string.IsNullOrWhiteSpace(name))
                    existing = Folders.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

                var local = existing ?? new Folder();

                local.Name = name;
                local.Handler = folder(local, new FolderHandlers());

                if (existing == null)
                    Folders = Folders.Append(local).ToArray();

                return local;
            }

            public void Remove(string name)
            {
                if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty", nameof(name));
                
                Remove(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            }

            public void Remove(Folder folder)
            {
                if (folder == null) throw new ArgumentNullException(nameof(folder));

                Remove(x => x.Equals(folder));
            }

            public void Remove(Func<Folder, bool> predicate)
            {
                if (predicate == null) throw new ArgumentNullException(nameof(predicate));

                EnsureFolders();
                Folders = Folders.Where(x => !predicate(x)).ToArray();
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
                    double totalSeconds = Math.Max(timeSpan.TotalSeconds, 0d);

                    return new FilesOlderThanHandler((uint)totalSeconds, searchPattern, includeSubDirectories);
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
                        .Where(x => Seconds == 0 || Time.UtcNow - x.LastWriteTimeUtc > TimeSpan.FromSeconds(Seconds));
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
                    return
	                    $"Files older than {Seconds} second(s){(SearchPattern != null ? $" (Search Pattern = {SearchPattern}" : string.Empty)}{(IncludeSubDirectories ? " (All directories)" : string.Empty)}.";
                }
            }
        }

        [Obsolete("Each individual archive now controls their own expiration.")]
        public TimeSpan CleanUpArchivesOlderThan { get; set; }
    }
}