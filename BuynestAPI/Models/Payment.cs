
using System.ComponentModel.DataAnnotations;

namespace BuynestAPI.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "Razorpay";

        [Required]
        public string Status { get; set; } = "Pending";

        public string? TransactionId { get; set; }

        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Order? Order { get; set; }
    }
}
