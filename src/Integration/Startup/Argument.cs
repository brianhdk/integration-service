using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Vertica.Integration.Startup
{
    internal class Argument
    {
        private readonly Func<string, bool> _parser;
        private readonly Func<string, string> _error;
        private readonly string _example;

        private Argument(Func<string, bool> parser, Func<string, string> error, string example)
        {
            _parser = parser;
            _error = error;
            _example = example;
        }

        public string Example
        {
            get { return _example; }
        }

        public bool IsValid(string value)
        {
            return _parser(value);
        }

        public bool IsValid(string value, out string error)
        {
            error = null;

            if (IsValid(value))
                return true;

            error = _error(value);
            return false;
        }

        public static Argument AbsoluteUrl
        {
            get
            {
                Func<string, bool> parser = value =>
                {
                    Uri result;
                    return
                        Uri.TryCreate(value, UriKind.Absolute, out result) ||
                        Regex.IsMatch(value ?? String.Empty, @"^http(?:s)?://\+(?:\:\d+)?/?$", RegexOptions.IgnoreCase);
                };

                Func<string, string> error = value => String.Format("'{0}' is not a valid absolute url.", value);

                return new Argument(parser, error, "http://localhost:8080");
            }
        }

        public static Argument Number(uint min = 1, uint max = UInt32.MaxValue)
        {
            Func<string, bool> parser = value =>
            {
                byte result;
                return byte.TryParse(value, out result) && result >= min && result <= max;
            };

            Func<string, string> error = value =>
                String.Format("'{0}' is not a valid repeating interval in seconds between [{1},{2}].", value, min, max);

            string example = min.ToString();

            return new Argument(parser, error, example);
        }

        public static Argument Any(params Argument[] arguments)
        {
            arguments = arguments ?? new Argument[0];

            Func<string, bool> parser = value => arguments.Any(x => x.IsValid(value));
            Func<string, string> error = value => String.Format("'{0}' value is not an acceptable argument.", value);
 
            return new Argument(parser, error, String.Format("[{0}]", String.Join(" | ", arguments.Select(x => x.Example))));
        }
    }
}