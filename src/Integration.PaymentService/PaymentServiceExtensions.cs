using System;

namespace Vertica.Integration.PaymentService
{
    public static class PaymentServiceExtensions
    {
        public static ApplicationConfiguration UsePaymentService(this ApplicationConfiguration application, Action<PaymentServiceConfiguration> paymentService = null)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

            return application.Extensibility(extensibility =>
            {
                PaymentServiceConfiguration configuration = extensibility.Register(() => new PaymentServiceConfiguration(application));

                paymentService?.Invoke(configuration);
            });
        }
    }
}