using Payments.POS.Models;

namespace Payments.POS.Services
{
    public class PaymentApiService
    {
        private readonly HttpClient _httpClient;

        public PaymentApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CreatePaymentResponse> CreatePaymentAsync(decimal amount)
        {
            var response = await _httpClient.PostAsJsonAsync("api/payments/create", new CreatePaymentRequest
            {
                Amount = amount
            });
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<CreatePaymentResponse>();
        }

        public async Task<PaymentDto> ApprovePaymentAsync(string paymentId)
        {
            return await PostActionAsync(paymentId, "approve");
        }

        public async Task<PaymentDto> DeclinePaymentAsync(string paymentId)
        {
            return await PostActionAsync(paymentId, "decline");
        }

        public async Task<PaymentDto> CancelPaymentAsync(string paymentId)
        {
            return await PostActionAsync(paymentId, "cancel");
        }

        private async Task<PaymentDto> PostActionAsync(string paymentId, string action)
        {
            var response = await _httpClient.PostAsync($"api/payments/{paymentId}/{action}", null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PaymentDto>();
        }
    }
}
