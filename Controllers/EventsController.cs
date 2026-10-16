using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public EventsController(RaceDayDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Event>>> GetAll() =>
        await _db.Events.Include(e => e.Organiser).Include(e => e.Categories).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Event>> GetById(int id)
    {
        var ev = await _db.Events
            .Include(e => e.Organiser)
            .Include(e => e.Categories)
            .FirstOrDefaultAsync(e => e.EventId == id);

        return ev == null ? NotFound() : ev;
    }

    [HttpPost]
    public async Task<ActionResult<Event>> Create(Event ev)
    {
        if (!new[] { "Run", "Walk", "Cycle" }.Contains(ev.EventType))
            return BadRequest("EventType must be Run, Walk, or Cycle.");

        if (ev.Distance <= 0)
            return BadRequest("Distance must be greater than 0.");

        if (!await _db.Users.AnyAsync(u => u.UserId == ev.OrganiserId))
            return BadRequest("Organiser does not exist.");

        ev.Organiser = null;      // avoid re-inserting nav property
        ev.Categories.Clear();

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, ev);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Event updated)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();

        ev.Name = updated.Name;
        ev.Description = updated.Description;
        ev.Date = updated.Date;
        ev.Location = updated.Location;
        ev.Distance = updated.Distance;
        ev.EventType = updated.EventType;
        ev.BannerImageUrl = updated.BannerImageUrl;
        ev.OrganiserId = updated.OrganiserId;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}