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

        public string Command { get; private set; }
        public Arguments CommandArgs { get; private set; }
        public Arguments Args { get; private set; }

	    public override string ToString()
	    {
		    return $"{Command} [CommandArgs = {CommandArgs}] [Args = {Args}]";
	    }
    }
}