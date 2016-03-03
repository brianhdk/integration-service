using System.Collections.Generic;

namespace Vertica.Integration.Model.Hosting
{
    public class HostArguments
    {
        public HostArguments(string command, KeyValuePair<string, string>[] commandArgs, KeyValuePair<string, string>[] args)
        {
            Command = command;

            CommandArgs = new Arguments("-", commandArgs);
            Args = new Arguments(string.Empty, args);
        }

        public string Command { get; }
        public Arguments CommandArgs { get; }
        public Arguments Args { get; }

	    public override string ToString()
	    {
		    return $"{Command} [CommandArgs = {CommandArgs}] [Args = {Args}]";
	    }
    }
}