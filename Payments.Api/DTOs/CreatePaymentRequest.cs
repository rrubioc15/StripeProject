using System.ComponentModel.DataAnnotations;

namespace Payments.Api.DTOs
{
    public class CreatePaymentRequest
    {
        [Range(0.5, double.MaxValue, ErrorMessage = "El monto mínimo es 0.50 USD.")]
        public decimal Amount { get; set; }
    }
}
