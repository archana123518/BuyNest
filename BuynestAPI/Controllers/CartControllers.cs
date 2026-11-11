
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BuynestAPI.Data;
using BuynestAPI.Models;
using System.Security.Claims;

namespace BuynestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // Add product to cart
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(
            [FromBody] AddToCartRequest request)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            if (request.Quantity < 1)
                return BadRequest("Quantity must be at least 1");

            var product = await _context.Products
                .FindAsync(request.ProductId);

            if (product == null)
                return NotFound("Product not found");

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.ProductId == request.ProductId);

            int newQuantity =
                (existingItem?.Quantity ?? 0) + request.Quantity;

            if (newQuantity > product.StockQuantity)
                return BadRequest(
                    "Requested quantity exceeds available stock");

            if (existingItem != null)
            {
                existingItem.Quantity = newQuantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                };

                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Product added to cart successfully"
            });
        }

        // View cart items
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToListAsync();

            return Ok(cartItems);
        }

        // Remove item from cart
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c =>
                    c.Id == id && c.UserId == userId);

            if (cartItem == null)
                return NotFound("Cart item not found");

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return Ok("Item removed from cart");
        }

        // Update cart item quantity
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCartQuantity(
            int id,
            [FromBody] AddToCartRequest request)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            if (request.Quantity < 1)
                return BadRequest("Quantity must be at least 1");

            var cartItem = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c =>
                    c.Id == id && c.UserId == userId);

            if (cartItem == null)
                return NotFound("Cart item not found");

            if (cartItem.Product == null)
                return NotFound("Product not found");

            if (request.Quantity > cartItem.Product.StockQuantity)
                return BadRequest(
                    "Requested quantity exceeds available stock");

            cartItem.Quantity = request.Quantity;

            await _context.SaveChangesAsync();

            return Ok(cartItem);
        }
    }
}
