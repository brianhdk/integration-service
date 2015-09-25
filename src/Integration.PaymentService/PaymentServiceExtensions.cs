using System;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.PaymentService.Clients.Dibs;

namespace Vertica.Integration.PaymentService
{
    public static class PaymentServiceExtensions
    {
        public static ApplicationConfiguration UsePaymentService(this ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException("application");

            application.AddCustomInstaller(Install.ByConvention
                .AddFromAssemblyOfThis<ITransactionStatusService>());

            return application;
        }
    }
}