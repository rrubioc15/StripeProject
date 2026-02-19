namespace Payments.Api.Models
{
    public class Payment
    {
        public string PaymentId { get; set; }
        public string PaymentIntentId { get; set; }
        public string Status { get; set; }
    }
}
