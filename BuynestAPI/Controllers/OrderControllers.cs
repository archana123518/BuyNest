
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
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/Orders - Place order with delivery address
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(
            [FromBody] DeliveryAddressRequest request)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToListAsync();

            if (cartItems.Count == 0)
                return BadRequest("Your cart is empty");

            if (cartItems.Any(c => c.Product == null ||
                c.Quantity < 1 ||
                c.Quantity > 100 ||
                c.Product.Price <= 0 ||
                c.Product.StockQuantity < c.Quantity))
            {
                return BadRequest(
                    "A cart item is invalid or has insufficient stock");
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",

                TotalAmount = cartItems.Sum(c =>
                    c.Product!.Price * c.Quantity),

                // Delivery address
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                HouseNumber = request.HouseNumber,
                Street = request.Street,
                City = request.City,
                State = request.State,
                Pincode = request.Pincode,
                DeliveryInstructions = request.DeliveryInstructions
            };

            foreach (var cartItem in cartItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Product!.Price
                });

                cartItem.Product!.StockQuantity -=
                    cartItem.Quantity;
            }

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            return Ok(order);
        }

        // GET: api/Orders - Get logged-in customer's orders
        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Payments)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var result = orders.Select(o => new
            {
                o.Id,
                o.UserId,
                o.OrderDate,
                o.TotalAmount,
                o.Status,
                o.ExpectedDeliveryDate,

                o.FullName,
                o.PhoneNumber,
                o.HouseNumber,
                o.Street,
                o.City,
                o.State,
                o.Pincode,
                o.DeliveryInstructions,

                OrderItems = o.OrderItems,

                PaymentStatus = o.Payments
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => p.Status)
                    .FirstOrDefault(),

                PaymentMethod = o.Payments
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => p.PaymentMethod)
                    .FirstOrDefault()
            });

            return Ok(result);
        }

        // GET: api/Orders/admin - Get all customer orders
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrdersForAdmin()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Payments)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }

        // PUT: api/Orders/{id}/status - Update order status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(
            int id,
            [FromBody] string status)
        {
            var allowedStatuses = new[]
            {
                "Pending",
                "Confirmed",
                "Ordered",
                "Packed",
                "Shipped",
                "Out for Delivery",
                "Delivered"
            };

            if (!allowedStatuses.Contains(status))
                return BadRequest("Invalid order status");

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound("Order not found");

            order.Status = status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Order status updated successfully",
                orderId = order.Id,
                status = order.Status
            });
        }

        // PUT: api/Orders/{id}/delivery-date
        [HttpPut("{id}/delivery-date")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDeliveryDate(
            int id,
            [FromBody] DateTime? deliveryDate)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound("Order not found");

            if (deliveryDate.HasValue &&
                deliveryDate.Value.Date < DateTime.UtcNow.Date)
            {
                return BadRequest(
                    "Delivery date cannot be in the past");
            }

            order.ExpectedDeliveryDate = deliveryDate?.Date;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Delivery date updated successfully",
                orderId = order.Id,
                expectedDeliveryDate = order.ExpectedDeliveryDate
            });
        }

        // PUT: api/Orders/2/cancel - Cancel order
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o =>
                    o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound("Order not found");

  var cancellableStatuses = new[]
{
    "Pending",
    "Confirmed",
    "Ordered",
    "Packed"
};

if (!cancellableStatuses.Contains(order.Status))
    return BadRequest("Order cannot be cancelled after shipping");

            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products
                    .FindAsync(item.ProductId);

                if (product != null)
                    product.StockQuantity += item.Quantity;
            }

            order.Status = "Cancelled";

            await _context.SaveChangesAsync();

            return Ok("Order cancelled successfully");
        }

        // DELETE: api/Orders/2 - Delete order
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o =>
                    o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound("Order not found");

            if (order.Status != "Cancelled")
                return BadRequest(
                    "Cancel the order before deleting it");

            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();

            return Ok("Order deleted successfully");
        }
    }
}
