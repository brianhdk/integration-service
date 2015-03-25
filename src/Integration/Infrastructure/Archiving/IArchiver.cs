using System;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public interface IArchiver
    {
        BeginArchive Create(string name, Action<ArchiveCreated> onCreated);

        Archive[] GetAll();
        byte[] Get(string id);

        int Delete(DateTimeOffset olderThan);
    }
}