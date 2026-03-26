using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Facilities;
using CollegeScheduler.Data.Entities.Requests;
using CollegeScheduler.Data.Entities.Scheduling;
using CollegeScheduler.Data.Identity;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
public sealed class RequestScheduleChangeController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public RequestScheduleChangeController(ApplicationDbContext db)
	{
		_db = db;
	}

	// LIST
	[HttpGet("api/v1/admin/request-schedule-changes")]
	public async Task<ActionResult<PagedResult<RequestScheduleChangeDto>>> GetAll([FromQuery] RequestScheduleChangeQuery q)
	{
		var query = _db.Set<RequestScheduleChange>()
			.AsNoTracking()
			.AsQueryable();

		if (q.RequestId.HasValue)
			query = query.Where(x => x.RequestId == q.RequestId.Value);

		if (q.TimetableEventId.HasValue)
			query = query.Where(x => x.TimetableEventId == q.TimetableEventId.Value);

		if (q.ProposedRoomId.HasValue)
			query = query.Where(x => x.ProposedRoomId == q.ProposedRoomId.Value);

		if (q.IsActive.HasValue)
			query = query.Where(x => x.IsActive == q.IsActive.Value);

		var sortBy = (q.SortBy ?? "createdat").Trim().ToLowerInvariant();
		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

		query = sortBy switch
		{
			"timetableeventid" => desc ? query.OrderByDescending(x => x.TimetableEventId) : query.OrderBy(x => x.TimetableEventId),
			"proposedstartutc" => desc ? query.OrderByDescending(x => x.ProposedStartUtc) : query.OrderBy(x => x.ProposedStartUtc),
			"proposedendutc" => desc ? query.OrderByDescending(x => x.ProposedEndUtc) : query.OrderBy(x => x.ProposedEndUtc),
			_ => desc ? query.OrderByDescending(x => x.CreatedAtUtc) : query.OrderBy(x => x.CreatedAtUtc),
		};

		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new RequestScheduleChangeDto
			{
				RequestId = x.RequestId,
				TimetableEventId = x.TimetableEventId,
				ProposedRoomId = x.ProposedRoomId,
				ProposedStartUtc = x.ProposedStartUtc,
				ProposedEndUtc = x.ProposedEndUtc,
				Reason = x.Reason
			})
			.ToListAsync();

		return Ok(new PagedResult<RequestScheduleChangeDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	// GET by request id
	[HttpGet("api/v1/admin/request-schedule-changes/{requestId:long}")]
	public async Task<ActionResult<RequestScheduleChangeDto>> GetById(long requestId)
	{
		var item = await _db.Set<RequestScheduleChange>()
			.AsNoTracking()
			.Where(x => x.RequestId == requestId)
			.Select(x => new RequestScheduleChangeDto
			{
				RequestId = x.RequestId,
				TimetableEventId = x.TimetableEventId,
				ProposedRoomId = x.ProposedRoomId,
				ProposedStartUtc = x.ProposedStartUtc,
				ProposedEndUtc = x.ProposedEndUtc,
				Reason = x.Reason
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	// CREATE
	[HttpPost("api/v1/admin/request-schedule-changes")]
	public async Task<ActionResult<RequestScheduleChangeDto>> Create([FromBody] RequestScheduleChangeCreateDto dto, [FromQuery] long requestId)
	{
		var request = await _db.Set<Request>().FirstOrDefaultAsync(x => x.RequestId == requestId);
		if (request is null) return NotFound($"Request {requestId} not found.");

		var timetableEventExists = await _db.Set<TimetableEvent>().AnyAsync(x => x.TimetableEventId == dto.TimetableEventId);
		if (!timetableEventExists) return NotFound($"TimetableEvent {dto.TimetableEventId} not found.");

		if (dto.ProposedRoomId.HasValue)
		{
			var roomExists = await _db.Set<Room>().AnyAsync(x => x.RoomId == dto.ProposedRoomId.Value);
			if (!roomExists) return NotFound($"Room {dto.ProposedRoomId.Value} not found.");
		}

		var alreadyExists = await _db.Set<RequestScheduleChange>().AnyAsync(x => x.RequestId == requestId);
		if (alreadyExists) return Conflict($"RequestScheduleChange for Request {requestId} already exists.");

		var entity = new RequestScheduleChange
		{
			RequestId = requestId,
			TimetableEventId = dto.TimetableEventId,
			ProposedRoomId = dto.ProposedRoomId,
			ProposedStartUtc = dto.ProposedStartUtc,
			ProposedEndUtc = dto.ProposedEndUtc,
			Reason = dto.Reason,
			IsActive = true
		};

		_db.Add(entity);
		await _db.SaveChangesAsync();

		return CreatedAtAction(nameof(GetById), new { requestId = entity.RequestId }, new RequestScheduleChangeDto
		{
			RequestId = entity.RequestId,
			TimetableEventId = entity.TimetableEventId,
			ProposedRoomId = entity.ProposedRoomId,
			ProposedStartUtc = entity.ProposedStartUtc,
			ProposedEndUtc = entity.ProposedEndUtc,
			Reason = entity.Reason
		});
	}

	// UPDATE
	[HttpPut("api/v1/admin/request-schedule-changes/{requestId:long}")]
	public async Task<IActionResult> Update(long requestId, [FromBody] RequestScheduleChangeUpdateDto dto)
	{
		var entity = await _db.Set<RequestScheduleChange>().FirstOrDefaultAsync(x => x.RequestId == requestId);
		if (entity is null) return NotFound();

		var timetableEventExists = await _db.Set<TimetableEvent>().AnyAsync(x => x.TimetableEventId == dto.TimetableEventId);
		if (!timetableEventExists) return NotFound($"TimetableEvent {dto.TimetableEventId} not found.");

		if (dto.ProposedRoomId.HasValue)
		{
			var roomExists = await _db.Set<Room>().AnyAsync(x => x.RoomId == dto.ProposedRoomId.Value);
			if (!roomExists) return NotFound($"Room {dto.ProposedRoomId.Value} not found.");
		}

		entity.TimetableEventId = dto.TimetableEventId;
		entity.ProposedRoomId = dto.ProposedRoomId;
		entity.ProposedStartUtc = dto.ProposedStartUtc;
		entity.ProposedEndUtc = dto.ProposedEndUtc;
		entity.Reason = dto.Reason;
		entity.IsActive = dto.IsActive;

		await _db.SaveChangesAsync();
		return NoContent();
	}
}