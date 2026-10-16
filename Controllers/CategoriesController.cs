using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers;
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public CategoriesController(RaceDayDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetAll() =>
        await _db.Categories.Include(c => c.Event).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Category>> GetById(int id)
    {
        var cat = await _db.Categories.Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryId == id);
        return cat == null ? NotFound() : cat;
    }

    [HttpGet("by-event/{eventId:int}")]
    public async Task<ActionResult<IEnumerable<Category>>> GetByEvent(int eventId) =>
        await _db.Categories.Where(c => c.EventId == eventId).ToListAsync();

    [HttpPost]
    public async Task<ActionResult<Category>> Create(Category category)
    {
        if (!await _db.Events.AnyAsync(e => e.EventId == category.EventId))
            return BadRequest("Event does not exist.");

        category.Event = null;

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, category);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cat = await _db.Categories.FindAsync(id);
        if (cat == null) return NotFound();

        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}