using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities;
using Vertica.Utilities.Extensions.StringExt;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public class FileBasedArchiveService : IArchiveService
	{
		private readonly DirectoryInfo _baseDirectory;
		private readonly ILogger _logger;

        internal const string BaseDirectoryKey = "FileBasedArchiveService.BaseDirectory";

		public FileBasedArchiveService(IRuntimeSettings settings, ILogger logger)
		{
		    _baseDirectory = new DirectoryInfo(settings[BaseDirectoryKey].NullIfEmpty() ?? "Data\\Archives");
			_logger = logger;
		}

	    public BeginArchive Create(string name, Action<ArchiveCreated> onCreated = null)
		{
			return new BeginArchive(name, (stream, options) =>
			{
				string archiveId = CreateNewArchiveId();

				FileInfo filePath = new FileInfo(Path.Combine(EnsureBaseDirectory().FullName, $"{archiveId}.zip"));

				File.WriteAllBytes(filePath.FullName, stream.ToArray());
				File.WriteAllText(MetaFilePath(filePath).FullName, new MetaFile(options).ToString());

                onCreated?.Invoke(new ArchiveCreated(archiveId));
            });
		}

	    protected virtual string CreateNewArchiveId()
	    {
	        return Guid.NewGuid().ToString("N");
	    }

	    public Archive[] GetAll()
		{
			return Archives().Select(Map).ToArray();
		}

		public byte[] Get(string id)
		{
		    if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(@"Value cannot be null or empty", nameof(id));
		    
			string filePath = Path.Combine(_baseDirectory.FullName, $"{id}.zip");

			if (!File.Exists(filePath))
				return null;

			return File.ReadAllBytes(filePath);
		}

		public int Delete(DateTimeOffset olderThan)
		{
			int deleted = 0;

			foreach (FileInfo archiveFile in Archives())
			{
				if (archiveFile.CreationTimeUtc <= olderThan)
				{
					DeleteArchive(archiveFile);
					deleted++;
				}
			}

			return deleted;
		}

		public int DeleteExpired()
		{
			int deleted = 0;

			foreach (FileInfo archiveFile in Archives())
			{
				if (Map(archiveFile).Expires.GetValueOrDefault(DateTimeOffset.MaxValue) <= Time.UtcNow)
				{
					DeleteArchive(archiveFile);
					deleted++;
				}
			}

			return deleted;
		}

        private DirectoryInfo EnsureBaseDirectory()
        {
            if (!_baseDirectory.Exists)
                _baseDirectory.Create();

            return _baseDirectory;
        }

        private static void DeleteArchive(FileInfo archiveFile)
		{
			FileInfo metaFile = MetaFilePath(archiveFile);

			archiveFile.Delete();

			if (metaFile.Exists)
				metaFile.Delete();
		}

		private IEnumerable<FileInfo> Archives()
		{
			return EnsureBaseDirectory().EnumerateFiles("*.zip", SearchOption.AllDirectories);
		}

		private Archive Map(FileInfo archiveFile)
		{
			MetaFile metaFile = ReadMetaFile(archiveFile);

			return new Archive
			{
				Id = Path.GetFileNameWithoutExtension(archiveFile.Name),
				Created = archiveFile.CreationTimeUtc,
				ByteSize = archiveFile.Length,
				Name = metaFile != null ? metaFile.Name : archiveFile.Name,
				GroupName = metaFile?.GroupName,
				Expires = metaFile?.Expires
			};
		}

		private MetaFile ReadMetaFile(FileInfo archiveFile)
		{
			FileInfo filePath = MetaFilePath(archiveFile);

			if (!filePath.Exists)
				return null;

			string text = File.ReadAllText(filePath.FullName);

			if (string.IsNullOrWhiteSpace(text))
				return null;

			return MetaFile.FromJson(text, _logger);
		}

		private static FileInfo MetaFilePath(FileInfo archiveFile)
		{
			return new FileInfo(Path.Combine($"{archiveFile.FullName}.meta"));
		}

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
		private class MetaFile
		{
			public MetaFile(ArchiveOptions options = null)
			{
				if (options != null)
				{
					Name = options.Name;
					GroupName = options.GroupName;
					Expires = options.Expires;					
				}
			}

			public string Name { get; set; }
			public string GroupName { get; set; }
			public DateTimeOffset? Expires { get; set; }

			public override string ToString()
			{
				return JsonConvert.SerializeObject(this, Formatting.Indented);
			}

			public static MetaFile FromJson(string json, ILogger logger)
			{
				if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(json));
				if (logger == null) throw new ArgumentNullException(nameof(logger));

				try
				{
					return JsonConvert.DeserializeObject<MetaFile>(json);
				}
				catch (Exception ex)
				{
					logger.LogError(ex);

					return null;
				}
			}
		}
	}
}