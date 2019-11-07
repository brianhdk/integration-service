using Vertica.Integration.Infrastructure.Archiving;

namespace Vertica.Integration.Perfion
{
    public class QueryArchiveOptions : ArchiveOptions
    {
        public QueryArchiveOptions(string name)
            : base(name)
        {
        }

        public void Disable()
        {
            Disabled = true;
        }

        internal bool Disabled { get; private set; }
    }
}