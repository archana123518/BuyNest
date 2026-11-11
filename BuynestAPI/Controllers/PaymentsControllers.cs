
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuynestAPI.Models;
using BuynestAPI.Data;
using Microsoft.AspNetCore.Authorization;

namespace BuynestAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment(
            [FromBody] PaymentRequest request)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == request.OrderId);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            if (request.Amount != order.TotalAmount)
                return BadRequest(new { message = "Payment amount does not match order total" });

            var payment = new Payment
            {
                OrderId = order.Id,
                PaymentMethod = "Test Payment",
                Amount = order.TotalAmount,
                Status = "Pending"
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = payment.Id,
                orderId = payment.OrderId,
                amount = payment.Amount,
                status = payment.Status
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(int id)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
                return NotFound(new { message = "Payment not found" });

            return Ok(payment);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdatePaymentStatus(
            int id,
            [FromBody] string status)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
                return NotFound(new { message = "Payment not found" });

            if (status != "Pending" &&
                status != "Success" &&
                status != "Failed")
            {
                return BadRequest(new { message = "Invalid payment status" });
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == payment.OrderId);

            if (order == null)
                return NotFound(new { message = "Related order not found" });

            payment.Status = status;

            // COD order remains pending until delivery.
            // Test payment success confirms the test order.
            if (payment.PaymentMethod != "Cash on Delivery")
            {
                order.Status = status == "Success"
                    ? "Confirmed"
                    : status;
            }
            else
            {
                order.Status = "Confirmed";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Payment status updated",
                paymentId = payment.Id,
                orderId = order.Id,
                paymentStatus = payment.Status,
                orderStatus = order.Status
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
                return NotFound(new { message = "Payment not found" });

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Payment deleted", paymentId = id });
        }
    }
}
