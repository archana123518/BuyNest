
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuynestAPI.Data;
using BuynestAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace BuynestAPI.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }

     
[Authorize(Roles = "Admin")]
[HttpPost]
public async Task<ActionResult<Product>> AddProduct(Product product)
{
    _context.Products.Add(product);
    await _context.SaveChangesAsync();

    return Ok(product);
}


[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteProduct(int id)
{
    var product = await _context.Products.FindAsync(id);

    if (product == null)
    {
        return NotFound();
    }

    _context.Products.Remove(product);
    await _context.SaveChangesAsync();

    return Ok("Product deleted successfully");
}

[Authorize(Roles = "Admin")]
[HttpPut("{id}")]
public async Task<IActionResult> UpdateProduct(int id, Product updatedProduct)
{
    var product = await _context.Products.FindAsync(id);

    if (product == null)
    {
        return NotFound();
    }

    product.Name = updatedProduct.Name;
    product.Description = updatedProduct.Description;
    product.Price = updatedProduct.Price;
    product.DiscountPercentage = updatedProduct.DiscountPercentage;
    product.StockQuantity = updatedProduct.StockQuantity;
    product.ImageUrl = updatedProduct.ImageUrl;
   product.CategoryId = updatedProduct.CategoryId;

    await _context.SaveChangesAsync();

    return Ok(product);
}



    }
}




