using Vertica.Integration.Model;
using Vertica.Integration.PaymentService;
using Vertica.PaymentService.Clients.Dibs;

namespace Vertica.Integration.Experiments
{
    public static class PaymentServiceTester
    {
        public static ApplicationConfiguration TestPaymentService(this ApplicationConfiguration application)
        {
            return application
                .UsePaymentService()
                .Fast()
                .Tasks(tasks => tasks
                    .Clear()
                    .Task<PaymentServiceTesterTask>());
        }
    }

    public class PaymentServiceTesterTask : Task
    {
        private readonly ITransactionStatusService _status;

        public PaymentServiceTesterTask(ITransactionStatusService status)
        {
            _status = status;
        }

        public override string Description
        {
            get { return "Tests payment service"; }
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            TransactionStatusResponse require = _status.Execute(new TransactionStatusRequest(
                "90196944",
                "TRANSID",
                "ORDERNUMBER",
                Currency.USD, // This is currently hard-coded, as we don't have any other currencies than USD.
                100m));

            context.Log.Message(require.Status);
        }
    }
}