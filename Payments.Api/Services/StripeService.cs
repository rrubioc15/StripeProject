using Microsoft.Extensions.Options;
using Payments.Api.Configuration;
using Stripe;

namespace Payments.Api.Services
{
    public class StripeService
    {

        public StripeService(IOptions<StripeConfig> options) {

            StripeConfiguration.ApiKey = options.Value.SecretKey;

        }

        public async Task<PaymentIntent> CreatePaymentIntent(long amount, string paymentId)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = amount,
                    Currency = "usd",
                    PaymentMethodTypes = new List<string> { "card" },
                    Metadata = new Dictionary<string, string>
                {
                    { "paymentId", paymentId }
                }
                };

                var requestOptions = new RequestOptions
                {
                    IdempotencyKey = paymentId
                };

                var service = new PaymentIntentService();
                return await service.CreateAsync(options, requestOptions);
            }
            catch (StripeException ex)
            {
                throw new ApplicationException($"Ocurrió un error: {ex.Message}");
            }
        }

        public async Task<PaymentIntent> Confirm(string intentId, string paymentMethod)
        {
            try
            {
                var service = new PaymentIntentService();

                return await service.ConfirmAsync(intentId,
                    new PaymentIntentConfirmOptions
                    {
                        PaymentMethod = paymentMethod
                    });
            }
            catch (StripeException ex)
            {
                if (ex.StripeError?.DeclineCode == "generic_decline")
                {
                    return ex.StripeError.PaymentIntent;
                }

                throw new ApplicationException($"Ocurrió un error: {ex.Message}");
            }
        }

        public async Task Cancel(string intentId)
        {
            try
            {
                var service = new PaymentIntentService();
                await service.CancelAsync(intentId);
            }
            catch (StripeException ex)
            {
                throw new ApplicationException($"Ocurrió un error: {ex.Message}");
            }
        }

        public string MapStatus(string stripeStatus)
            => stripeStatus switch
            {
                "succeeded" => "APPROVED",
                "requires_payment_method" => "DECLINED",
                "canceled" => "FAILED",
                "processing" => "PENDING",
                _ => stripeStatus
            };
    }
}
