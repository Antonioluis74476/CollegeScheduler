using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Scheduling;
using CollegeScheduler.Data.Identity;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Scheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
public sealed class EventStatusController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public EventStatusController(ApplicationDbContext db) => _db = db;

	[HttpGet("api/v1/admin/event-statuses")]
	public async Task<ActionResult<PagedResult<EventStatusDto>>> GetAll([FromQuery] EventStatusQuery q)
	{
		var query = _db.Set<EventStatus>().AsNoTracking();

		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			var s = q.Search.Trim();
			query = query.Where(x => x.Name.Contains(s));
		}

		query = query.OrderBy(x => x.Name);

		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new EventStatusDto { EventStatusId = x.EventStatusId, Name = x.Name })
			.ToListAsync();

		return Ok(new PagedResult<EventStatusDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	[HttpGet("api/v1/admin/event-statuses/{id:int}")]
	public async Task<ActionResult<EventStatusDto>> GetById(int id)
	{
		var item = await _db.Set<EventStatus>()
			.AsNoTracking()
			.Where(x => x.EventStatusId == id)
			.Select(x => new EventStatusDto { EventStatusId = x.EventStatusId, Name = x.Name })
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	[HttpPost("api/v1/admin/event-statuses")]
	public async Task<ActionResult<EventStatusDto>> Create([FromBody] EventStatusCreateDto dto)
	{
		if (string.IsNullOrWhiteSpace(dto.Name))
			return BadRequest("Name is required.");

		var entity = new EventStatus { Name = dto.Name.Trim() };
		_db.Add(entity);

		try { await _db.SaveChangesAsync(); }
		catch (DbUpdateException) { return Conflict("Could not save EventStatus. Name must be unique."); }

		return CreatedAtAction(nameof(GetById), new { id = entity.EventStatusId },
			new EventStatusDto { EventStatusId = entity.EventStatusId, Name = entity.Name });
	}

	[HttpPut("api/v1/admin/event-statuses/{id:int}")]
	public async Task<IActionResult> Update(int id, [FromBody] EventStatusUpdateDto dto)
	{
		if (string.IsNullOrWhiteSpace(dto.Name))
			return BadRequest("Name is required.");

		var entity = await _db.Set<EventStatus>().FirstOrDefaultAsync(x => x.EventStatusId == id);
		if (entity is null) return NotFound();

		entity.Name = dto.Name.Trim();

		try { await _db.SaveChangesAsync(); }
		catch (DbUpdateException) { return Conflict("Could not update EventStatus. Name must be unique."); }

		return NoContent();
	}

	[HttpDelete("api/v1/admin/event-statuses/{id:int}")]
	public async Task<IActionResult> Delete(int id)
	{
		var entity = await _db.Set<EventStatus>().FindAsync(id);
		if (entity is null) return NotFound();

		_db.Remove(entity);

		try { await _db.SaveChangesAsync(); }
		catch (DbUpdateException) { return Conflict("Cannot delete EventStatus because it is in use."); }

		return NoContent();
	}
}