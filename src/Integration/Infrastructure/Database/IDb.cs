using System;
using System.Collections.Generic;

namespace Vertica.Integration.Infrastructure.Database
{
    public interface IDb : IDisposable
    {
        IEnumerable<T> Query<T>(string sql, params object[] args);
        List<T> Fetch<T>(string sql, params object[] args);
        T SingleOrDefault<T>(string sql, params object[] args);        
    }
}