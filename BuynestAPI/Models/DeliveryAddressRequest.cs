
using System.ComponentModel.DataAnnotations;

namespace BuynestAPI.Models
{
    public class DeliveryAddressRequest
    {
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
    }
}
