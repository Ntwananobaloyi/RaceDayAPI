using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers;
[ApiController]
[Route("api/[controller]")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public EnrolmentsController(RaceDayDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Enrolment>>> GetAll() =>
        await _db.Enrolments
            .Include(e => e.Participant)
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Include(e => e.Result)
            .ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Enrolment>> GetById(int id)
    {
        var enr = await _db.Enrolments
            .Include(e => e.Participant)
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Include(e => e.Result)
            .FirstOrDefaultAsync(e => e.EnrolmentId == id);

        return enr == null ? NotFound() : enr;
    }

    [HttpGet("by-participant/{participantId:int}")]
    public async Task<ActionResult<IEnumerable<Enrolment>>> GetByParticipant(int participantId) =>
        await _db.Enrolments
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Where(e => e.ParticipantId == participantId)
            .ToListAsync();

    [HttpPost]
    public async Task<ActionResult<Enrolment>> Create(Enrolment enrolment)
    {
        if (await _db.Enrolments.AnyAsync(e =>
                e.ParticipantId == enrolment.ParticipantId && e.EventId == enrolment.EventId))
            return Conflict("Participant is already enrolled in this event.");

        if (!await _db.Users.AnyAsync(u => u.UserId == enrolment.ParticipantId))
            return BadRequest("Participant does not exist.");

        var category = await _db.Categories.FindAsync(enrolment.CategoryId);
        if (category == null) return BadRequest("Category does not exist.");
        if (category.EventId != enrolment.EventId)
            return BadRequest("Category does not belong to that event.");

        enrolment.Participant = null;
        enrolment.Event = null;
        enrolment.Category = null;
        enrolment.Result = null;
        enrolment.Status = "Pending";
        enrolment.EnrolmentDate = DateTime.UtcNow;

        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = enrolment.EnrolmentId }, enrolment);
    }

    [HttpPatch("{id:int}/confirm")]
    public async Task<IActionResult> Confirm(int id)
    {
        var enr = await _db.Enrolments.FindAsync(id);
        if (enr == null) return NotFound();

        enr.Status = "Confirmed";
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var enr = await _db.Enrolments.FindAsync(id);
        if (enr == null) return NotFound();

        _db.Enrolments.Remove(enr);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}