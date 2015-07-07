using System;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.PaymentService.Clients.Dibs;

namespace Vertica.Integration.PaymentService
{
    public static class PaymentServiceExtensions
    {
        public static ApplicationConfiguration UsePaymentService(this ApplicationConfiguration builder)
        {
            if (builder == null) throw new ArgumentNullException("builder");

            builder.AddCustomInstaller(new ConventionInstaller()
                .AddFromAssemblyOfThis<ITransactionStatusService>());

            return builder;
        }
    }
}