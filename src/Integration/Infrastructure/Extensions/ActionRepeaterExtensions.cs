using System;
using System.IO;

namespace Vertica.Integration.Infrastructure.Extensions
{
    public static class ActionRepeaterExtensions
    {
        public static ActionRepeater Repeat(this Action action, Delay delay, TextWriter outputter = null)
        {
            return ActionRepeater.Start(action, delay, outputter);
        }
    }
}