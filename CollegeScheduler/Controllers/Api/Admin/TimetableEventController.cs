using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Academic;
using CollegeScheduler.Data.Entities.Facilities;
using CollegeScheduler.Data.Entities.Scheduling;
using CollegeScheduler.Data.Identity;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Scheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Route("api/v1/admin/timetable-events")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class TimetableEventController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public TimetableEventController(ApplicationDbContext db)
	{
		_db = db;
	}

	[HttpGet]
	public async Task<ActionResult<PagedResult<TimetableEventDto>>> GetAll([FromQuery] TimetableEventQuery q)
	{
		var query = _db.TimetableEvents.AsNoTracking();

		if (q.TermId.HasValue) query = query.Where(x => x.TermId == q.TermId.Value);
		if (q.ModuleId.HasValue) query = query.Where(x => x.ModuleId == q.ModuleId.Value);
		if (q.RoomId.HasValue) query = query.Where(x => x.RoomId == q.RoomId.Value);
		if (q.EventStatusId.HasValue) query = query.Where(x => x.EventStatusId == q.EventStatusId.Value);
		if (q.RecurrenceGroupId.HasValue) query = query.Where(x => x.RecurrenceGroupId == q.RecurrenceGroupId.Value);

		if (q.FromUtc.HasValue) query = query.Where(x => x.EndUtc >= q.FromUtc.Value);
		if (q.ToUtc.HasValue) query = query.Where(x => x.StartUtc <= q.ToUtc.Value);

		query = query.OrderBy(x => x.StartUtc).ThenBy(x => x.TimetableEventId);

		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new TimetableEventDto
			{
				TimetableEventId = x.TimetableEventId,
				TermId = x.TermId,
				ModuleId = x.ModuleId,
				RoomId = x.RoomId,
				StartUtc = x.StartUtc,
				EndUtc = x.EndUtc,
				EventStatusId = x.EventStatusId,
				SessionType = x.SessionType,
				RecurrenceGroupId = x.RecurrenceGroupId,
				Notes = x.Notes,
				CreatedByUserId = x.CreatedByUserId,
				CreatedAtUtc = x.CreatedAtUtc,
				UpdatedAtUtc = x.UpdatedAtUtc
			})
			.ToListAsync();

		return Ok(new PagedResult<TimetableEventDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	[HttpGet("{id:long}")]
	public async Task<ActionResult<TimetableEventDto>> GetById(long id)
	{
		var item = await _db.TimetableEvents
			.AsNoTracking()
			.Where(x => x.TimetableEventId == id)
			.Select(x => new TimetableEventDto
			{
				TimetableEventId = x.TimetableEventId,
				TermId = x.TermId,
				ModuleId = x.ModuleId,
				RoomId = x.RoomId,
				StartUtc = x.StartUtc,
				EndUtc = x.EndUtc,
				EventStatusId = x.EventStatusId,
				SessionType = x.SessionType,
				RecurrenceGroupId = x.RecurrenceGroupId,
				Notes = x.Notes,
				CreatedByUserId = x.CreatedByUserId,
				CreatedAtUtc = x.CreatedAtUtc,
				UpdatedAtUtc = x.UpdatedAtUtc
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	[HttpPost]
	public async Task<ActionResult<TimetableEventDto>> Create([FromBody] TimetableEventCreateDto dto)
	{
		if (dto.EndUtc <= dto.StartUtc)
			return BadRequest("EndUtc must be greater than StartUtc.");

		var termExists = await _db.Terms.AnyAsync(t => t.TermId == dto.TermId);
		if (!termExists) return NotFound($"Term {dto.TermId} not found.");

		var moduleExists = await _db.Modules.AnyAsync(m => m.ModuleId == dto.ModuleId);
		if (!moduleExists) return NotFound($"Module {dto.ModuleId} not found.");

		var roomExists = await _db.Rooms.AnyAsync(r => r.RoomId == dto.RoomId);
		if (!roomExists) return NotFound($"Room {dto.RoomId} not found.");

		var statusExists = await _db.EventStatuses.AnyAsync(s => s.EventStatusId == dto.EventStatusId);
		if (!statusExists) return NotFound($"EventStatus {dto.EventStatusId} not found.");

		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrWhiteSpace(userId))
			return Unauthorized("Missing user id claim.");

		var now = DateTime.UtcNow;

		var entity = new TimetableEvent
		{
			TermId = dto.TermId,
			ModuleId = dto.ModuleId,
			RoomId = dto.RoomId,
			StartUtc = dto.StartUtc,
			EndUtc = dto.EndUtc,
			EventStatusId = dto.EventStatusId,
			SessionType = string.IsNullOrWhiteSpace(dto.SessionType) ? "Lecture" : dto.SessionType.Trim(),
			RecurrenceGroupId = dto.RecurrenceGroupId,
			Notes = dto.Notes,
			CreatedByUserId = userId,
			CreatedAtUtc = now,
			UpdatedAtUtc = now
		};

		_db.TimetableEvents.Add(entity);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not save TimetableEvent. A conflicting record may already exist.");
		}

		return CreatedAtAction(nameof(GetById), new { id = entity.TimetableEventId }, new TimetableEventDto
		{
			TimetableEventId = entity.TimetableEventId,
			TermId = entity.TermId,
			ModuleId = entity.ModuleId,
			RoomId = entity.RoomId,
			StartUtc = entity.StartUtc,
			EndUtc = entity.EndUtc,
			EventStatusId = entity.EventStatusId,
			SessionType = entity.SessionType,
			RecurrenceGroupId = entity.RecurrenceGroupId,
			Notes = entity.Notes,
			CreatedByUserId = entity.CreatedByUserId,
			CreatedAtUtc = entity.CreatedAtUtc,
			UpdatedAtUtc = entity.UpdatedAtUtc
		});
	}

	[HttpPut("{id:long}")]
	public async Task<IActionResult> Update(long id, [FromBody] TimetableEventUpdateDto dto)
	{
		if (dto.EndUtc <= dto.StartUtc)
			return BadRequest("EndUtc must be greater than StartUtc.");

		var entity = await _db.TimetableEvents.FirstOrDefaultAsync(x => x.TimetableEventId == id);
		if (entity is null) return NotFound();

		var termExists = await _db.Terms.AnyAsync(t => t.TermId == dto.TermId);
		if (!termExists) return NotFound($"Term {dto.TermId} not found.");

		var moduleExists = await _db.Modules.AnyAsync(m => m.ModuleId == dto.ModuleId);
		if (!moduleExists) return NotFound($"Module {dto.ModuleId} not found.");

		var roomExists = await _db.Rooms.AnyAsync(r => r.RoomId == dto.RoomId);
		if (!roomExists) return NotFound($"Room {dto.RoomId} not found.");

		var statusExists = await _db.EventStatuses.AnyAsync(s => s.EventStatusId == dto.EventStatusId);
		if (!statusExists) return NotFound($"EventStatus {dto.EventStatusId} not found.");

		entity.TermId = dto.TermId;
		entity.ModuleId = dto.ModuleId;
		entity.RoomId = dto.RoomId;
		entity.StartUtc = dto.StartUtc;
		entity.EndUtc = dto.EndUtc;
		entity.EventStatusId = dto.EventStatusId;
		entity.SessionType = dto.SessionType.Trim();
		entity.RecurrenceGroupId = dto.RecurrenceGroupId;
		entity.Notes = dto.Notes;
		entity.UpdatedAtUtc = DateTime.UtcNow;

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not update TimetableEvent. A conflicting record may already exist.");
		}

		return NoContent();
	}

	[HttpDelete("{id:long}")]
	public async Task<IActionResult> Delete(long id)
	{
		var entity = await _db.TimetableEvents.FindAsync(id);
		if (entity is null) return NotFound();

		_db.TimetableEvents.Remove(entity);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Cannot delete TimetableEvent because it is referenced by other rows (cohorts/lecturers/changes). Delete joins first.");
		}

		return NoContent();
	}
}