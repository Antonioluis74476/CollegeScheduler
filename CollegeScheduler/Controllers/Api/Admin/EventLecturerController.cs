using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Profiles;
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
public sealed class EventLecturerController : ControllerBase
{
	private readonly ApplicationDbContext _db;
	public EventLecturerController(ApplicationDbContext db) => _db = db;

	[HttpGet("api/v1/admin/event-lecturers")]
	public async Task<ActionResult<PagedResult<EventLecturerDto>>> GetAll([FromQuery] EventLecturerQuery q)
	{
		var query = _db.Set<EventLecturer>().AsNoTracking();

		if (q.TimetableEventId.HasValue) query = query.Where(x => x.TimetableEventId == q.TimetableEventId.Value);
		if (q.LecturerId.HasValue) query = query.Where(x => x.LecturerId == q.LecturerId.Value);

		query = query.OrderBy(x => x.TimetableEventId).ThenBy(x => x.LecturerId);

		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new EventLecturerDto { TimetableEventId = x.TimetableEventId, LecturerId = x.LecturerId })
			.ToListAsync();

		return Ok(new PagedResult<EventLecturerDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	[HttpPost("api/v1/admin/event-lecturers")]
	public async Task<ActionResult<EventLecturerDto>> Create([FromBody] EventLecturerCreateDto dto)
	{
		var eventExists = await _db.Set<TimetableEvent>().AnyAsync(e => e.TimetableEventId == dto.TimetableEventId);
		if (!eventExists) return NotFound($"TimetableEvent {dto.TimetableEventId} not found.");

		var lecturerExists = await _db.Set<LecturerProfile>().AnyAsync(l => l.LecturerId == dto.LecturerId);
		if (!lecturerExists) return NotFound($"Lecturer {dto.LecturerId} not found.");

		var entity = new EventLecturer { TimetableEventId = dto.TimetableEventId, LecturerId = dto.LecturerId };
		_db.Add(entity);

		try { await _db.SaveChangesAsync(); }
		catch (DbUpdateException) { return Conflict("Could not save EventLecturer. (TimetableEventId, LecturerId) must be unique."); }

		return Ok(new EventLecturerDto { TimetableEventId = entity.TimetableEventId, LecturerId = entity.LecturerId });
	}

	[HttpDelete("api/v1/admin/event-lecturers/{timetableEventId:long}/{lecturerId:int}")]
	public async Task<IActionResult> Delete(long timetableEventId, int lecturerId)
	{
		var entity = await _db.Set<EventLecturer>()
			.FirstOrDefaultAsync(x => x.TimetableEventId == timetableEventId && x.LecturerId == lecturerId);

		if (entity is null) return NotFound();

		_db.Remove(entity);
		await _db.SaveChangesAsync();
		return NoContent();
	}
}