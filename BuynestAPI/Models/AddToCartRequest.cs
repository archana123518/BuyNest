
using System.ComponentModel.DataAnnotations;

namespace BuynestAPI.Models
{
    public class AddToCartRequest
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; } = 1;
    }
}
