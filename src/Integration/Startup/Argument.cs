using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Vertica.Integration.Startup
{
    internal class Argument
    {
        private readonly Func<string, bool> _parser;
        private readonly Func<string, string> _error;

        private Argument(Func<string, bool> parser, Func<string, string> error)
        {
            _parser = parser;
            _error = error;
        }

        public static Argument AbsoluteUrl
        {
            get
            {
                return new Argument(value =>
                {
                    Uri result;
                    return 
                        Uri.TryCreate(value, UriKind.Absolute, out result) ||
                        Regex.IsMatch(value ?? String.Empty, @"^http(?:s)?://\+(?:\:\d+)?/?$", RegexOptions.IgnoreCase);

                }, value => String.Format("'{0}' is not a valid absolute url.", value));
            }
        }

        public static Argument Byte(byte min = 0, byte max = 255)
        {
            return new Argument(value =>
            {
                byte result;
                return byte.TryParse(value, out result) && result >= min && result <= max;

            }, value => String.Format("'{0}' is not a valid repeating interval in seconds [{1}-{2}].", value, min, max));
        }

        public static Argument Any(params Argument[] arguments)
        {
            return new Argument(value => (arguments ?? new Argument[0]).Any(x => x.IsValid(value)), 
                value => String.Format("'{0}' value is not an acceptable argument.", value));
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
    }
}