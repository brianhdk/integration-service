using System;

namespace Vertica.Integration.Model.Web
{
    internal interface IWebApiControllers
    {
        Type[] Controllers { get; }
    }
}