using System;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public interface IArchiver
    {
        Archive Create(string name, Action<string> onCreated);

        SavedArchive[] GetAll();
        byte[] Get(string id);

        int Delete(DateTime olderThan);
    }
}