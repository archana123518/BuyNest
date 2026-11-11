
using System.ComponentModel.DataAnnotations;

namespace BuynestAPI.Models
{
    public class PaymentRequest
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        [Range(1, 1000000)]
        public decimal Amount { get; set; }
    }
}
