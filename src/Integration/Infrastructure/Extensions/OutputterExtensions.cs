using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Vertica.Integration.Infrastructure.Extensions
{
	public static class OutputterExtensions
	{
		public static string ReadLine(this TextWriter outputter, string message = "Enter value: ")
		{
			if (outputter == null) throw new ArgumentNullException(nameof(outputter));

			if (!string.IsNullOrWhiteSpace(message))
				outputter.Write(message);

			if (!Environment.UserInteractive)
				return null;

			return Console.ReadLine();
		}

		/// <summary>
		/// Like System.Console.ReadLine(), only with a mask.
		/// </summary>
		/// <param name="outputter">Writer that outputs initial string</param>
		/// <param name="message">Message to show user before entering password.</param>
		/// <param name="mask">a <c>char</c> representing your choice of console mask</param>
		/// <returns>the string the user typed in </returns>
		public static string ReadPassword(this TextWriter outputter, string message = "Enter password: ", char mask = '*')
		{
			if (outputter == null) throw new ArgumentNullException(nameof(outputter));

			if (!string.IsNullOrWhiteSpace(message))
				outputter.Write(message);

			if (!Environment.UserInteractive)
				return null;

			const int enter = 13, backspace = 8, ctrlbackspace = 127;
			int[] filtered = { 0, 27, 9, 10 /*, 32 space, if you care */ };

			var pass = new Stack<char>();
			char c;

			while ((c = Console.ReadKey(true).KeyChar) != enter)
			{
				if (c == backspace)
				{
					if (pass.Count > 0)
					{
						outputter.Write("\b \b");
						pass.Pop();
					}
				}
				else if (c == ctrlbackspace)
				{
					while (pass.Count > 0)
					{
						outputter.Write("\b \b");
						pass.Pop();
					}
				}
				else if (filtered.Count(x => c == x) > 0)
				{
				}
				else
				{
					pass.Push(c);
					outputter.Write(mask);
				}
			}

			outputter.WriteLine();
			return new string(pass.Reverse().ToArray());
		}

		public static void RepeatUntilEscapeKeyIsHit(this TextWriter outputter, Action repeat)
		{
			if (outputter == null) throw new ArgumentNullException(nameof(outputter));
			if (repeat == null) throw new ArgumentNullException(nameof(repeat));

			do
			{
				repeat();

				outputter.WriteLine("Press ESCAPE to break or any key to continue");
				outputter.WriteLine();

			} while (WaitingForEscape());
		}

		public static void WaitUntilEscapeKeyIsHit(this TextWriter outputter, string message = "Press ESCAPE to continue...")
		{
			if (outputter == null) throw new ArgumentNullException(nameof(outputter));

			do
			{
				if (!string.IsNullOrWhiteSpace(message))
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