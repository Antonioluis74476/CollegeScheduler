using CollegeScheduler.Data;
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
[Authorize(Roles = RoleNames.Admin)]
public sealed class TimetableEventChangeController : ControllerBase
{
	private readonly ApplicationDbContext _db;
	public TimetableEventChangeController(ApplicationDbContext db) => _db = db;

	[HttpGet("api/v1/admin/timetable-event-changes")]
	public async Task<ActionResult<PagedResult<TimetableEventChangeDto>>> GetAll([FromQuery] TimetableEventChangeQuery q)
	{
		var query = _db.Set<TimetableEventChange>().AsNoTracking();

		if (q.TimetableEventId.HasValue) query = query.Where(x => x.TimetableEventId == q.TimetableEventId.Value);
		if (q.NotificationSent.HasValue) query = query.Where(x => x.NotificationSent == q.NotificationSent.Value);

		query = query.OrderByDescending(x => x.ChangedAtUtc);

		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new TimetableEventChangeDto
			{
				TimetableEventChangeId = x.TimetableEventChangeId,
				TimetableEventId = x.TimetableEventId,
				ChangeType = x.ChangeType,
				OldRoomId = x.OldRoomId,
				NewRoomId = x.NewRoomId,
				OldStartUtc = x.OldStartUtc,
				NewStartUtc = x.NewStartUtc,
				OldEndUtc = x.OldEndUtc,
				NewEndUtc = x.NewEndUtc,
				Reason = x.Reason,
				ChangedByUserId = x.ChangedByUserId,
				ChangedAtUtc = x.ChangedAtUtc,
				NotificationSent = x.NotificationSent
			})
			.ToListAsync();

		return Ok(new PagedResult<TimetableEventChangeDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	[HttpPost("api/v1/admin/timetable-event-changes")]
	public async Task<ActionResult<TimetableEventChangeDto>> Create([FromBody] TimetableEventChangeCreateDto dto)
	{
		if (string.IsNullOrWhiteSpace(dto.ChangeType))
			return BadRequest("ChangeType is required.");

		if (string.IsNullOrWhiteSpace(dto.Reason))
			return BadRequest("Reason is required.");

		var eventExists = await _db.Set<TimetableEvent>().AnyAsync(e => e.TimetableEventId == dto.TimetableEventId);
		if (!eventExists) return NotFound($"TimetableEvent {dto.TimetableEventId} not found.");

		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrWhiteSpace(userId))
			return Unauthorized("Missing user id claim.");

		var entity = new TimetableEventChange
		{
			TimetableEventId = dto.TimetableEventId,
			ChangeType = dto.ChangeType.Trim(),
			OldRoomId = dto.OldRoomId,
			NewRoomId = dto.NewRoomId,
			OldStartUtc = dto.OldStartUtc,
			NewStartUtc = dto.NewStartUtc,
			OldEndUtc = dto.OldEndUtc,
			NewEndUtc = dto.NewEndUtc,
			Reason = dto.Reason.Trim(),
			ChangedByUserId = userId,
			ChangedAtUtc = DateTime.UtcNow,
			NotificationSent = dto.NotificationSent ?? false
		};

		_db.Add(entity);
		await _db.SaveChangesAsync();

		return Ok(new TimetableEventChangeDto
		{
			TimetableEventChangeId = entity.TimetableEventChangeId,
			TimetableEventId = entity.TimetableEventId,
			ChangeType = entity.ChangeType,
			OldRoomId = entity.OldRoomId,
			NewRoomId = entity.NewRoomId,
			OldStartUtc = entity.OldStartUtc,
			NewStartUtc = entity.NewStartUtc,
			OldEndUtc = entity.OldEndUtc,
			NewEndUtc = entity.NewEndUtc,
			Reason = entity.Reason,
			ChangedByUserId = entity.ChangedByUserId,
			ChangedAtUtc = entity.ChangedAtUtc,
			NotificationSent = entity.NotificationSent
		});
	}
}