using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public ResultsController(RaceDayDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Result>>> GetAll() =>
        await _db.Results.Include(r => r.Enrolment).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Result>> GetById(int id)
    {
        var result = await _db.Results.Include(r => r.Enrolment)
            .FirstOrDefaultAsync(r => r.ResultId == id);
        return result == null ? NotFound() : result;
    }

    [HttpPost]
    public async Task<ActionResult<Result>> Create(Result result)
    {
        if (result.FinishPosition <= 0)
            return BadRequest("FinishPosition must be greater than 0.");

        if (!await _db.Enrolments.AnyAsync(e => e.EnrolmentId == result.EnrolmentId))
            return BadRequest("Enrolment does not exist.");

        if (await _db.Results.AnyAsync(r => r.EnrolmentId == result.EnrolmentId))
            return Conflict("A result already exists for this enrolment.");

        result.Enrolment = null;

        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = result.ResultId }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Result updated)
    {
        var result = await _db.Results.FindAsync(id);
        if (result == null) return NotFound();

        if (updated.FinishPosition <= 0)
            return BadRequest("FinishPosition must be greater than 0.");

        result.FinishTime = updated.FinishTime;
        result.FinishPosition = updated.FinishPosition;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _db.Results.FindAsync(id);
        if (result == null) return NotFound();

        _db.Results.Remove(result);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}