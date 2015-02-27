namespace Vertica.Integration.Infrastructure.Archiving
{
    public class Archiver : IArchiver
    {
        public Archive New()
        {
            return new Archive();
        }
    }
}