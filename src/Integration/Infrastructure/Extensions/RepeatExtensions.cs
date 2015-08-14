namespace Vertica.Integration.Infrastructure.Extensions
{
	public static class RepeatExtensions
	{
		public static Repeat Times(this uint number)
		{
			return Repeat.Times(number);
		}

		public static Repeat Times(this int number)
		{
			return Repeat.Times((uint)number);
		}
	}
}