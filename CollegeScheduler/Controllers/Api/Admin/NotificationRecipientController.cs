using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Notifications;
using CollegeScheduler.Data.Identity;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
public sealed class NotificationRecipientController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public NotificationRecipientController(ApplicationDbContext db)
	{
		_db = db;
	}

	// LIST
	[HttpGet("api/v1/admin/notification-recipients")]
	public async Task<ActionResult<PagedResult<NotificationRecipientDto>>> GetAll([FromQuery] NotificationRecipientQuery q)
	{
		var query = _db.Set<NotificationRecipient>()
			.AsNoTracking()
			.AsQueryable();

		if (q.NotificationId.HasValue)
			query = query.Where(x => x.NotificationId == q.NotificationId.Value);

		if (!string.IsNullOrWhiteSpace(q.UserId))
			query = query.Where(x => x.UserId == q.UserId);

		if (!string.IsNullOrWhiteSpace(q.DeliveryStatus))
			query = query.Where(x => x.DeliveryStatus == q.DeliveryStatus);

		if (q.IsActive.HasValue)
			query = query.Where(x => x.IsActive == q.IsActive.Value);

		var sortBy = (q.SortBy ?? "createdat").Trim().ToLowerInvariant();
		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

		query = sortBy switch
		{
			"notificationid" => desc ? query.OrderByDescending(x => x.NotificationId) : query.OrderBy(x => x.NotificationId),
			"userid" => desc ? query.OrderByDescending(x => x.UserId) : query.OrderBy(x => x.UserId),
			"deliverystatus" => desc ? query.OrderByDescending(x => x.DeliveryStatus) : query.OrderBy(x => x.DeliveryStatus),
			"sentat" => desc ? query.OrderByDescending(x => x.SentAtUtc) : query.OrderBy(x => x.SentAtUtc),
			"readat" => desc ? query.OrderByDescending(x => x.ReadAtUtc) : query.OrderBy(x => x.ReadAtUtc),
			_ => desc ? query.OrderByDescending(x => x.CreatedAtUtc) : query.OrderBy(x => x.CreatedAtUtc),
		};

		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new NotificationRecipientDto
			{
				NotificationId = x.NotificationId,
				UserId = x.UserId,
				DeliveryStatus = x.DeliveryStatus,
				SentAtUtc = x.SentAtUtc,
				ReadAtUtc = x.ReadAtUtc
			})
			.ToListAsync();

		return Ok(new PagedResult<NotificationRecipientDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	// GET by composite key
	[HttpGet("api/v1/admin/notification-recipients/{notificationId:long}/{userId}")]
	public async Task<ActionResult<NotificationRecipientDto>> GetById(long notificationId, string userId)
	{
		var item = await _db.Set<NotificationRecipient>()
			.AsNoTracking()
			.Where(x => x.NotificationId == notificationId && x.UserId == userId)
			.Select(x => new NotificationRecipientDto
			{
				NotificationId = x.NotificationId,
				UserId = x.UserId,
				DeliveryStatus = x.DeliveryStatus,
				SentAtUtc = x.SentAtUtc,
				ReadAtUtc = x.ReadAtUtc
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	// CREATE
	[HttpPost("api/v1/admin/notification-recipients")]
	public async Task<ActionResult<NotificationRecipientDto>> Create(
		[FromBody] NotificationRecipientCreateDto dto,
		[FromQuery] long notificationId)
	{
		var notificationExists = await _db.Set<Notification>().AnyAsync(x => x.NotificationId == notificationId);
		if (!notificationExists) return NotFound($"Notification {notificationId} not found.");

		var userExists = await _db.Users.AnyAsync(x => x.Id == dto.UserId);
		if (!userExists) return NotFound($"User {dto.UserId} not found.");

		var exists = await _db.Set<NotificationRecipient>()
			.AnyAsync(x => x.NotificationId == notificationId && x.UserId == dto.UserId);

		if (exists)
			return Conflict($"NotificationRecipient already exists for Notification {notificationId} and User {dto.UserId}.");

		var entity = new NotificationRecipient
		{
			NotificationId = notificationId,
			UserId = dto.UserId,
			DeliveryStatus = string.IsNullOrWhiteSpace(dto.DeliveryStatus) ? "Pending" : dto.DeliveryStatus,
			IsActive = true
		};

		_db.Add(entity);
		await _db.SaveChangesAsync();

		return CreatedAtAction(nameof(GetById), new { notificationId = entity.NotificationId, userId = entity.UserId }, new NotificationRecipientDto
		{
			NotificationId = entity.NotificationId,
			UserId = entity.UserId,
			DeliveryStatus = entity.DeliveryStatus,
			SentAtUtc = entity.SentAtUtc,
			ReadAtUtc = entity.ReadAtUtc
		});
	}

	// UPDATE
	[HttpPut("api/v1/admin/notification-recipients/{notificationId:long}/{userId}")]
	public async Task<IActionResult> Update(long notificationId, string userId, [FromBody] NotificationRecipientUpdateDto dto)
	{
		var entity = await _db.Set<NotificationRecipient>()
			.FirstOrDefaultAsync(x => x.NotificationId == notificationId && x.UserId == userId);

		if (entity is null) return NotFound();

		entity.DeliveryStatus = dto.DeliveryStatus;
		entity.SentAtUtc = dto.SentAtUtc;
		entity.ReadAtUtc = dto.ReadAtUtc;
		entity.IsActive = dto.IsActive;

		await _db.SaveChangesAsync();
		return NoContent();
	}
}