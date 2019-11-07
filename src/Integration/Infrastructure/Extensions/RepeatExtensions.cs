using System;

namespace Vertica.Integration.Infrastructure.Extensions
{
    [Obsolete("Will be removed.")]
	public static class RepeatExtensions
	{
        [Obsolete("Will be removed.")]
        public static Repeat Times(this uint number)
		{
			return Repeat.Times(number);
		}

        [Obsolete("Will be removed.")]
        public static Repeat Times(this int number)
		{
			return Repeat.Times((uint)number);
		}
	}
}