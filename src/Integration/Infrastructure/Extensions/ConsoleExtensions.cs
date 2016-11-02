using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vertica.Integration.Infrastructure.IO;

namespace Vertica.Integration.Infrastructure.Extensions
{
	public static class ConsoleExtensions
	{
		public static string ReadLine(this TextWriter writer, string message = "Enter value: ")
		{
			if (writer == null) throw new ArgumentNullException(nameof(writer));

			if (!string.IsNullOrWhiteSpace(message))
				writer.Write(message);

			if (!Environment.UserInteractive)
				return null;

			return Console.ReadLine();
		}

		/// <summary>
		/// Like System.Console.ReadLine(), only with a mask.
		/// </summary>
		/// <param name="writer">Writer that outputs initial string</param>
		/// <param name="message">Message to show user before entering password.</param>
		/// <param name="mask">a <c>char</c> representing your choice of console mask</param>
		/// <returns>the string the user typed in </returns>
		public static string ReadPassword(this TextWriter writer, string message = "Enter password: ", char mask = '*')
		{
			if (writer == null) throw new ArgumentNullException(nameof(writer));

			if (!string.IsNullOrWhiteSpace(message))
				writer.Write(message);

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
						writer.Write("\b \b");
						pass.Pop();
					}
				}
				else if (c == ctrlbackspace)
				{
					while (pass.Count > 0)
					{
						writer.Write("\b \b");
						pass.Pop();
					}
				}
				else if (filtered.Count(x => c == x) > 0)
				{
				}
				else
				{
					pass.Push(c);
					writer.Write(mask);
				}
			}

			writer.WriteLine();

			return new string(pass.Reverse().ToArray());
		}

		public static void RepeatUntilEscapeKeyIsHit(this IConsoleWriter console, Action repeat)
		{
			if (console == null) throw new ArgumentNullException(nameof(console));
			if (repeat == null) throw new ArgumentNullException(nameof(repeat));

			do
			{
				repeat();

                console.WriteLine("Press ESCAPE to break or any key to continue");
			    console.WriteLine();

			} while (WaitingForEscape());
		}

		public static void WaitUntilEscapeKeyIsHit(this IConsoleWriter console, string message = "Press ESCAPE to continue...")
		{
			if (console == null) throw new ArgumentNullException(nameof(console));

			do
			{
				if (!string.IsNullOrWhiteSpace(message))
				{
					console.WriteLine(message);
					console.WriteLine();					
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