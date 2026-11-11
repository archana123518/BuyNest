
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuynestAPI.Data;
using BuynestAPI.Models;

namespace BuynestAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        var categories = await _context.Categories.ToListAsync();
        return Ok(categories);
    }
    
    [HttpPost]
    public async Task<ActionResult<Category>> AddCategory(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return Ok(category);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, Category updatedCategory)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        category.Name = updatedCategory.Name;
        category.Description = updatedCategory.Description;

        await _context.SaveChangesAsync();

        return Ok(category);
    }
    
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteCategory(int id)
{
    var category = await _context.Categories.FindAsync(id);

    if (category == null)
    {
        return NotFound();
    }

    _context.Categories.Remove(category);

    await _context.SaveChangesAsync();

    return Ok("Category deleted successfully");
}

   


}
