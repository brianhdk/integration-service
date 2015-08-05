using System;
using System.Collections.Generic;
using System.Linq;
using Vertica.Integration.Infrastructure.Extensions;

namespace Vertica.Integration.Model.Hosting
{
    public class ArgumentsParser : IArgumentsParser
    {
        public HostArguments Parse(string[] arguments)
        {
            if (arguments == null) throw new ArgumentNullException("arguments");

	        string command = arguments.FirstOrDefault();
            var commandArgs = new List<KeyValuePair<string, string>>();
            var args = new List<KeyValuePair<string, string>>();

            var remaining = new Queue<string>(arguments.Skip(1));

            while (remaining.Count > 0)
            {
                string current = remaining.Dequeue();

                if (current.StartsWith("-"))
                {
                    if (current.Length > 1)
                        commandArgs.Add(Map(current.Substring(1), remaining));
                }
                else
                {
                    args.Add(Map(current, remaining));
                }
            }

            return new HostArguments(command, commandArgs.ToArray(), args.ToArray());
        }

        private static KeyValuePair<string, string> Map(string key, Queue<string> remaining)
        {
            string value = null;

            int indexOfDelimiter = key.IndexOf(":", StringComparison.InvariantCulture);

            if (indexOfDelimiter > 0)
            {
                value = key.Substring(indexOfDelimiter + 1);
                key = key.Substring(0, indexOfDelimiter);
            }
            else if (remaining.SafePeek(String.Empty).StartsWith(":"))
            {
                value = remaining.Dequeue().Substring(1);
            }

            if (value != null && value.Length == 0)
                value = remaining.Count > 0 ? remaining.Dequeue() : null;

            return new KeyValuePair<string, string>(key, value);
        }
    }
}