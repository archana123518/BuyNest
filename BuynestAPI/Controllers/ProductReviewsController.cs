using BuynestAPI.Data;
using BuynestAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BuynestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductReviewsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductReviewsController(AppDbContext context)
        {
            _context = context;
        }

 [HttpGet("product/{productId}")]
public async Task<IActionResult> GetProductReviews(int productId)
{
    var reviews = await _context.ProductReviews
        .Where(r => r.ProductId == productId)
        .OrderByDescending(r => r.CreatedAt)
        .ToListAsync();

    return Ok(reviews);
}
        // POST: api/ProductReviews
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddReview(ProductReview review)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found");
            }

            if (review.OrderId == null)
            {
                return BadRequest("Order ID is required");
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o =>
                    o.Id == review.OrderId &&
                    o.UserId == userId);

            if (order == null)
            {
                return NotFound("Order not found");
            }

            if (!string.Equals(order.Status, "Delivered",
                StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("You can review only delivered orders");
            }

            var productExists = order.OrderItems
                .Any(item => item.ProductId == review.ProductId);

            if (!productExists)
            {
                return BadRequest("This product is not part of the order");
            }

            var alreadyReviewed = await _context.ProductReviews
                .AnyAsync(r =>
                    r.OrderId == review.OrderId &&
                    r.ProductId == review.ProductId);

            if (alreadyReviewed)
            {
                return BadRequest("You have already reviewed this product");
            }

            review.ReviewerName = order.FullName;
            review.CreatedAt = DateTime.UtcNow;

            _context.ProductReviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(review);
        }
    }
}