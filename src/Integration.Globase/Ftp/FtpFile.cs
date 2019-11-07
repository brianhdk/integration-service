using System;
using Vertica.Utilities;

namespace Vertica.Integration.Globase.Ftp
{
	public class FtpFile
	{
		public FtpFile(string fileName, string contents)
		{
			if (string.IsNullOrWhiteSpace(contents)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(contents));
			if (string.IsNullOrWhiteSpace(contents)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(contents));

			FileName = fileName;
			Contents = contents;
		}

		public string FileName { get; }
		public string Contents { get; }

		public static FtpFile Csv(string prefix, string contents)
		{
			if (string.IsNullOrWhiteSpace(prefix)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(prefix));

			return new FtpFile($"{prefix}_{Time.Now:yyyyMMddHHmmss}.csv", contents);
		}
	}
}