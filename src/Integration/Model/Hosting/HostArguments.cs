using System;
using System.Collections.Generic;

namespace Vertica.Integration.Model.Hosting
{
    public class HostArguments
    {
        public HostArguments(string command, KeyValuePair<string, string>[] commandArgs, KeyValuePair<string, string>[] args)
        {
            Command = command;

            CommandArgs = new Arguments("-", commandArgs);
            Args = new Arguments(String.Empty, args);
        }

        public string Command { get; private set; }
        public Arguments CommandArgs { get; private set; }
        public Arguments Args { get; private set; }

	    public override string ToString()
	    {
		    return String.Format("{0} [CommandArgs = {1}] [Args = {2}]", Command, CommandArgs, Args);
	    }
    }
}