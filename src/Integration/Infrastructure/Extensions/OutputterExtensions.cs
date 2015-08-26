using System;
using System.IO;

namespace Vertica.Integration.Infrastructure.Extensions
{
	public static class OutputterExtensions
	{
		public static void RepeatUntilEscapeKeyIsHit(this TextWriter outputter, Action repeat)
		{
			if (outputter == null) throw new ArgumentNullException("outputter");
			if (repeat == null) throw new ArgumentNullException("repeat");

			do
			{
				repeat();

				outputter.WriteLine("Press ESCAPE to break or any key to continue");
				outputter.WriteLine();

			} while (WaitingForEscape());
		}

		public static void WaitUntilEscapeKeyIsHit(this TextWriter outputter, string message = "Press ESCAPE to continue...")
		{
			if (outputter == null) throw new ArgumentNullException("outputter");

			do
			{
				if (!String.IsNullOrWhiteSpace(message))
				{
					outputter.WriteLine(message);
					outputter.WriteLine();					
				}

			} while (WaitingForEscape());
		}

		private static bool WaitingForEscape()
		{
			// We can't do anything but to return true.
			if (!Environment.UserInteractive)
				return true;

			return Console.ReadKey(intercept: true /* don't display */).Key != ConsoleKey.Escape;
		}
	}
}