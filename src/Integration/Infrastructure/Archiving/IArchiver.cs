using System;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public interface IArchiver
    {
        Archive Create(string name, Action<int> onCreated);

        SavedArchive[] GetAll();
        byte[] Get(int id);
    }
}