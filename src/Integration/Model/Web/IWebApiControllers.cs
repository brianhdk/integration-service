using System;

namespace Vertica.Integration.Model.Web
{
    public interface IWebApiControllers
    {
        Type[] Controllers { get; }
    }
}