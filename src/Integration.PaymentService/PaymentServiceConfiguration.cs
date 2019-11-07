using System;
using Vertica.PaymentService.Clients.Dibs;

namespace Vertica.Integration.PaymentService
{
    public class PaymentServiceConfiguration
    {
        internal PaymentServiceConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

            Application = application
                .Services(services => services
                    .Conventions(conventions => conventions
                        .AddFromAssemblyOfThis<PaymentServiceConfiguration>()
                        .AddFromAssemblyOfThis<ITransactionStatusService>()));
        }

        public PaymentServiceConfiguration Change(Action<PaymentServiceConfiguration> change)
        {
            change?.Invoke(this);

            return this;
        }

        public ApplicationConfiguration Application { get; }
    }
}