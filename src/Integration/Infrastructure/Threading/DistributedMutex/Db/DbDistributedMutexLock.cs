using System;
using Vertica.Utilities;

namespace Vertica.Integration.Infrastructure.Threading.DistributedMutex.Db
{
    public class DbDistributedMutexLock : IEquatable<DbDistributedMutexLock>
    {
        protected DbDistributedMutexLock()
        {
        }

        public DbDistributedMutexLock(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty", nameof(name));

            Name = name;
            LockId = Guid.NewGuid();
            CreatedAt = Time.UtcNow;
        }

        public string Name { get; }
        public Guid LockId { get; }

        public DateTimeOffset CreatedAt { get; set; }

        public string MachineName { get; set; }
        public string Description { get; set; }

        public bool Equals(DbDistributedMutexLock other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) && LockId.Equals(other.LockId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DbDistributedMutexLock) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name.GetHashCode()*397) ^ LockId.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"Lock '{Name}' is currently acquired by '{MachineName}' at {CreatedAt}. Description: {Description ?? "<null>"}.";
        }
    }
}