namespace Vertica.Integration.Model.Startup
{
    internal class ArgumentValidator
    {
        private readonly Argument[] _arguments;

        public ArgumentValidator(string usage, params Argument[] arguments)
        {
            Usage = usage;
            _arguments = arguments ?? new Argument[0];
        }

        public string Usage { get; private set; }

        public Argument this[int index]
        {
            get { return _arguments[0]; }
        }

        public int Length { get { return _arguments.Length; } }

        public static ArgumentValidator WindowsService
        {
            get
            {
                return new ArgumentValidator(
                    "[absolute-url | number-of-seconds-to-delay-between-repeats]",
                    Argument.Either(Argument.AbsoluteUrl, Argument.Byte(min: 1)));                
            }
        }
    }
}