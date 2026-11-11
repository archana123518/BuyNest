using System.ComponentModel.DataAnnotations;

namespace BuynestAPI.Models;

public class Product
{
    public int Id{ get; set; }

    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "1000000",
        ErrorMessage = "Price must be greater than zero")]
    public decimal Price { get; set; }
    [Range(0, 90, ErrorMessage = "Discount must be between 0 and 90")]
public decimal DiscountPercentage { get; set; } = 0;

public decimal DiscountedPrice =>
    Price - (Price * DiscountPercentage / 100);

    [Range(0, 100000, ErrorMessage = "Stock quantity cannot be negative")]
    public int StockQuantity { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

 public int CategoryId { get; set; }

public Category? Category { get; set; } 
public ICollection<ProductReview> Reviews { get; set; }
    = new List<ProductReview>();
}