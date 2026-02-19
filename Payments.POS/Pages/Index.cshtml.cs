using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Payments.POS.Models;
using Payments.POS.Services;

namespace Payments.POS.Pages
{
    public class IndexModel : PageModel
    {
        private readonly PaymentApiService _paymentApiService;

        public IndexModel(PaymentApiService paymentApiService)
        {
            _paymentApiService = paymentApiService;
        }

        [BindProperty]
        public decimal Amount { get; set; }

        [BindProperty]
        public string PaymentId { get; set; }

        public string Status { get; set; }
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            PaymentId = null;
            Status = null;
            ErrorMessage = null;
            Amount = 0;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Amount <= 0)
            {
                ErrorMessage = "El monto debe ser mayor a 0.";
                return Page();
            }

            if (Amount != decimal.Round(Amount, 2))
            {
                ErrorMessage = "El monto no puede tener más de 2 decimales.";
                return Page();
            }

            try
            {
                var result = await _paymentApiService.CreatePaymentAsync(Amount);
                PaymentId = result.PaymentId;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error creando pago: {ex.Message}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync() => await HandleActionAsync("approve");
        public async Task<IActionResult> OnPostDeclineAsync() => await HandleActionAsync("decline");
        public async Task<IActionResult> OnPostCancelAsync() => await HandleActionAsync("cancel");

        private async Task<IActionResult> HandleActionAsync(string action)
        {
            try
            {
                PaymentDto payment = action switch
                {
                    "approve" => await _paymentApiService.ApprovePaymentAsync(PaymentId),
                    "decline" => await _paymentApiService.DeclinePaymentAsync(PaymentId),
                    "cancel" => await _paymentApiService.CancelPaymentAsync(PaymentId),
                    _ => throw new ArgumentException("Acción no válida")
                };

                Status = payment.Status;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error procesando pago: {ex.Message}";
            }

            return Page();
        }
    }
}
