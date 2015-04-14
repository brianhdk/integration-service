namespace Vertica.Integration.Infrastructure.IO
{
    public interface IDirectory
    {
        void Delete();

        // Todo: consider using same strategy as FTPClient
        //  - navigating files/directories
        //  - implement Create/Read/Update methods - using streams
    }
}