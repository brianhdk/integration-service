using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Vertica.Integration.Startup
{
    internal class ArgumentValidator : IEnumerable<Argument>
    {
        private readonly List<Argument> _arguments;

        public ArgumentValidator(string usage, params Argument[] arguments)
        {
            Usage = usage;
            _arguments = (arguments ?? new Argument[0]).ToList();
        }

        public string Usage { get; private set; }

        public Argument this[int index]
        {
            get { return _arguments[0]; }
        }

        public int Count { get { return _arguments.Count; } }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Argument> GetEnumerator()
        {
            return _arguments.GetEnumerator();
        }

        public static ArgumentValidator WindowsService
        {
            get
            {
                return new ArgumentValidator(
                    "[absolute-url | number-of-seconds-to-delay-between-repeats]",
                    Argument.Any(Argument.AbsoluteUrl, Argument.Number(min: 1)));                
            }
        }
    }
}