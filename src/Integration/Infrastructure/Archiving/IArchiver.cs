using System;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public interface IArchiver
    {
        BeginArchive Create(string name, Action<CreatedArchive> onCreated);

        Archive[] GetAll();
        byte[] Get(string id);

        int Delete(DateTimeOffset olderThan);
    }
}