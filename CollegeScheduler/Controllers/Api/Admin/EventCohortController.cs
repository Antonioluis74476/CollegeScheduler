using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Academic;
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
public sealed class EventCohortController : ControllerBase
{
	private readonly ApplicationDbContext _db;
	public EventCohortController(ApplicationDbContext db) => _db = db;

	[HttpGet("api/v1/admin/event-cohorts")]
	public async Task<ActionResult<PagedResult<EventCohortDto>>> GetAll([FromQuery] EventCohortQuery q)
	{
		var query = _db.Set<EventCohort>().AsNoTracking();

		if (q.TimetableEventId.HasValue) query = query.Where(x => x.TimetableEventId == q.TimetableEventId.Value);
		if (q.CohortId.HasValue) query = query.Where(x => x.CohortId == q.CohortId.Value);

		query = query.OrderBy(x => x.TimetableEventId).ThenBy(x => x.CohortId);

		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new EventCohortDto { TimetableEventId = x.TimetableEventId, CohortId = x.CohortId })
			.ToListAsync();

		return Ok(new PagedResult<EventCohortDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	[HttpPost("api/v1/admin/event-cohorts")]
	public async Task<ActionResult<EventCohortDto>> Create([FromBody] EventCohortCreateDto dto)
	{
		var eventExists = await _db.Set<TimetableEvent>().AnyAsync(e => e.TimetableEventId == dto.TimetableEventId);
		if (!eventExists) return NotFound($"TimetableEvent {dto.TimetableEventId} not found.");

		var cohortExists = await _db.Set<Cohort>().AnyAsync(c => c.CohortId == dto.CohortId);
		if (!cohortExists) return NotFound($"Cohort {dto.CohortId} not found.");

		var entity = new EventCohort { TimetableEventId = dto.TimetableEventId, CohortId = dto.CohortId };
		_db.Add(entity);

		try { await _db.SaveChangesAsync(); }
		catch (DbUpdateException) { return Conflict("Could not save EventCohort. (TimetableEventId, CohortId) must be unique."); }

		return Ok(new EventCohortDto { TimetableEventId = entity.TimetableEventId, CohortId = entity.CohortId });
	}

	[HttpDelete("api/v1/admin/event-cohorts/{timetableEventId:long}/{cohortId:int}")]
	public async Task<IActionResult> Delete(long timetableEventId, int cohortId)
	{
		var entity = await _db.Set<EventCohort>()
			.FirstOrDefaultAsync(x => x.TimetableEventId == timetableEventId && x.CohortId == cohortId);

		if (entity is null) return NotFound();

		_db.Remove(entity);
		await _db.SaveChangesAsync();
		return NoContent();
	}
}