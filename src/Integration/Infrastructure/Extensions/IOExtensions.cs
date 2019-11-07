using System;
using System.IO;

namespace Vertica.Integration.Infrastructure.Extensions
{
    public static class IOExtensions
    {
        public static bool IsLocked(this FileInfo file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                stream?.Close();
            }

            //file is not locked
            return false;
        }
    }
}