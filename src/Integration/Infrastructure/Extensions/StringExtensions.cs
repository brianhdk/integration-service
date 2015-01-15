namespace Vertica.Integration.Infrastructure.Extensions
{
    public static class StringExtensions
	{
		public static string MaxLength(this string value, uint maxLength)
		{
			return value != null && value.Length > maxLength ? value.Substring(0, (int)maxLength) : value;
		}
	}
}