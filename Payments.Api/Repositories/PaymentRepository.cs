using Payments.Api.Models;
using System.Collections.Concurrent;

namespace Payments.Api.Repositories
{
    public class PaymentRepository
    {
        public static ConcurrentDictionary<string, Payment> Payments = new();
    }
}
