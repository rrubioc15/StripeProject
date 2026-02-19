using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Payments.Api.DTOs;
using Payments.Api.Models;
using Payments.Api.Repositories;
using Payments.Api.Services;

namespace Payments.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly StripeService _stripeService;

        public PaymentsController (StripeService stripeService)
        {
            _stripeService = stripeService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreatePaymentRequest request)
        {
            if (decimal.Round(request.Amount, 2) != request.Amount)
            {
                ModelState.AddModelError("Amount", "Solo se permiten 2 decimales.");
                return ValidationProblem(ModelState);
            }

            long amountMinorUnits = (long)(request.Amount * 100);

            //Clave única para identificar el intento
            var paymentId = Guid.NewGuid().ToString();

            var intent = await _stripeService.CreatePaymentIntent(amountMinorUnits, paymentId);

            PaymentRepository.Payments[paymentId] = new Payment
            {
                PaymentId = paymentId,
                PaymentIntentId = intent.Id,
                Status = "PENDING"
            };

            return Ok(new { paymentId });
        }

        [HttpPost("{paymentId}/approve")]
        public async Task<IActionResult> Approve(string paymentId)
        {
            var payment = GetPendingPayment(paymentId);

            var intent = await _stripeService.Confirm(payment.PaymentIntentId, "pm_card_visa");

            payment.Status = _stripeService.MapStatus(intent.Status);

            return Ok(payment);
        }

        [HttpPost("{paymentId}/decline")]
        public async Task<IActionResult> Decline(string paymentId)
        {
            var payment = GetPendingPayment(paymentId);

            var intent = await _stripeService.Confirm(payment.PaymentIntentId, "pm_card_chargeDeclined");

            payment.Status = _stripeService.MapStatus(intent.Status);

            return Ok(payment);
        }

        [HttpPost("{paymentId}/cancel")]
        public async Task<IActionResult> Cancel(string paymentId)
        {
            var payment = GetPendingPayment(paymentId);

            await _stripeService.Cancel(payment.PaymentIntentId);

            payment.Status = "FAILED";

            return Ok(payment);
        }


        [HttpGet("{paymentId}")]
        public IActionResult Get(string paymentId)
        {
            if (!PaymentRepository.Payments.TryGetValue(paymentId, out var payment))
                return NotFound("Pago no encontrado. ");

            return Ok(payment);
        }


        private Payment GetPendingPayment(string paymentId)
        {
            if (!PaymentRepository.Payments.TryGetValue(paymentId, out var payment))
                throw new KeyNotFoundException("Pago no encontrado. ");

            if (payment.Status != "PENDING")
                throw new BadHttpRequestException("El pago ya fue procesado. ");

            return payment;
        }

    }
}
