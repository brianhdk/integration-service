using System;

namespace Vertica.Integration.WebApi.Infrastructure.Castle.Windsor
{
    internal interface IWebApiControllers
    {
        Type[] Controllers { get; }
    }
}