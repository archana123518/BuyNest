
using System.ComponentModel.DataAnnotations;

namespace BuynestAPI.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";
        public DateTime? ExpectedDeliveryDate { get; set; }

        // DELIVERY ADDRESS

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[6-9]\d{9}$",
            ErrorMessage = "Enter a valid 10-digit mobile number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string HouseNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Street { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[1-9][0-9]{5}$",
            ErrorMessage = "Enter a valid 6-digit PIN code")]
        public string Pincode { get; set; } = string.Empty;

        public string? DeliveryInstructions { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new();

        public List<Payment> Payments { get; set; } = new();
    }
}
