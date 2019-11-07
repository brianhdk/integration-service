using System;

namespace Vertica.Integration.Infrastructure.Extensions
{
    internal static class StringExtensions
	{
		public static string MaxLength(this string value, uint maxLength)
		{
			return value != null && value.Length > maxLength ? value.Substring(0, (int)maxLength) : value;
		}
        
	    // https://stackoverflow.com/questions/14488796/does-net-provide-an-easy-way-convert-bytes-to-kb-mb-gb-etc
	    private static readonly string[] SizeSuffixes = { "byte(s)", "KB", "MB", "GB", "TB", "PB", "EB" };

	    public static string ToPrettyFileSize(this long value)
	    {
	        if (value < 0L)
	            return $"-{ToPrettyFileSize(-value)}";

	        // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
	        int mag = (int)Math.Log(value, 1024);

	        // 1L << (mag * 10) == 2 ^ (10 * mag) 
	        // [i.e. the number of bytes in the unit corresponding to mag]
	        decimal adjustedSize = (decimal)value / (1L << (mag * 10));

	        return $"{Math.Round(adjustedSize, 0)} {SizeSuffixes[mag]}";
	    }
    }
}