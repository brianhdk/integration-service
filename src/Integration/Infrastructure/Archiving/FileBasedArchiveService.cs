using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities_v4;
using Vertica.Utilities_v4.Extensions.StringExt;

namespace Vertica.Integration.Infrastructure.Archiving
{
	internal class FileBasedArchiveService : IArchiveService
	{
		private readonly string _baseDirectory;
		private readonly ILogger _logger;

		public const string BaseDirectoryKey = "FileBasedArchiveService.BaseDirectory";

		public FileBasedArchiveService(IRuntimeSettings settings, ILogger logger)
		{
			_baseDirectory = settings[BaseDirectoryKey].NullIfEmpty() ?? "Data\\Archives";

			if (!Directory.Exists(_baseDirectory))
				Directory.CreateDirectory(_baseDirectory);

			_logger = logger;
		}

		public BeginArchive Create(string name, Action<ArchiveCreated> onCreated = null)
		{
			return new BeginArchive(name, (stream, options) =>
			{
				string archiveId = Guid.NewGuid().ToString("N");

				Directory.CreateDirectory(_baseDirectory);

				FileInfo filePath = new FileInfo(Path.Combine(_baseDirectory, String.Format("{0}.zip", archiveId)));

				File.WriteAllBytes(filePath.FullName, stream.ToArray());
				File.WriteAllText(MetaFilePath(filePath).FullName, new MetaFile(options).ToString());

				if (onCreated != null)
					onCreated(new ArchiveCreated(archiveId));
			});
		}

		public Archive[] GetAll()
		{
			return Archives().Select(Map).ToArray();
		}

		public byte[] Get(string id)
		{
			string filePath = Path.Combine(_baseDirectory, String.Format("{0}.zip", id));

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

		private static void DeleteArchive(FileInfo archiveFile)
		{
			FileInfo metaFile = MetaFilePath(archiveFile);

			archiveFile.Delete();

			if (metaFile.Exists)
				metaFile.Delete();
		}

		private IEnumerable<FileInfo> Archives()
		{
			return new DirectoryInfo(_baseDirectory).EnumerateFiles("*.zip", SearchOption.AllDirectories);
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
				GroupName = metaFile != null ? metaFile.GroupName : null,
				Expires = metaFile != null ? metaFile.Expires : null
			};
		}

		private MetaFile ReadMetaFile(FileInfo archiveFile)
		{
			FileInfo filePath = MetaFilePath(archiveFile);

			if (!filePath.Exists)
				return null;

			string text = File.ReadAllText(filePath.FullName);

			if (String.IsNullOrWhiteSpace(text))
				return null;

			return MetaFile.FromJson(text, _logger);
		}

		private static FileInfo MetaFilePath(FileInfo archiveFile)
		{
			return new FileInfo(Path.Combine(String.Format("{0}.meta", archiveFile.FullName)));
		}

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
				if (String.IsNullOrWhiteSpace(json)) throw new ArgumentException(@"Value cannot be null or empty.", "json");
				if (logger == null) throw new ArgumentNullException("logger");

				try
				{
					return JsonConvert.DeserializeAnonymousType(json, new MetaFile());
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