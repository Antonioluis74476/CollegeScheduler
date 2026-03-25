using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Notifications;
using CollegeScheduler.Data.Entities.Requests;
using CollegeScheduler.Data.Entities.Scheduling;
using CollegeScheduler.Data.Identity;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
public sealed class NotificationController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public NotificationController(ApplicationDbContext db)
	{
		_db = db;
	}

	[HttpGet("api/v1/admin/notifications")]
	public async Task<ActionResult<PagedResult<NotificationDto>>> GetAll([FromQuery] NotificationQuery q)
	{
		var query = _db.Set<Notification>()
			.AsNoTracking()
			.Include(x => x.Recipients)
			.AsQueryable();

		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			var s = q.Search.Trim();
			query = query.Where(x => x.Title.Contains(s) || x.Message.Contains(s));
		}

		if (q.NotificationTypeId.HasValue)
			query = query.Where(x => x.NotificationTypeId == q.NotificationTypeId.Value);

		if (q.RelatedTimetableEventId.HasValue)
			query = query.Where(x => x.RelatedTimetableEventId == q.RelatedTimetableEventId.Value);

		if (q.RelatedRequestId.HasValue)
			query = query.Where(x => x.RelatedRequestId == q.RelatedRequestId.Value);

		if (q.IsActive.HasValue)
			query = query.Where(x => x.IsActive == q.IsActive.Value);

		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
		query = desc ? query.OrderByDescending(x => x.CreatedAtUtc) : query.OrderBy(x => x.CreatedAtUtc);

		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new NotificationDto
			{
				NotificationId = x.NotificationId,
				NotificationTypeId = x.NotificationTypeId,
				Title = x.Title,
				Message = x.Message,
				RelatedTimetableEventId = x.RelatedTimetableEventId,
				RelatedRequestId = x.RelatedRequestId,
				IsActive = x.IsActive,
				Recipients = x.Recipients.Select(r => new NotificationRecipientDto
				{
					NotificationId = r.NotificationId,
					UserId = r.UserId,
					DeliveryStatus = r.DeliveryStatus,
					SentAtUtc = r.SentAtUtc,
					ReadAtUtc = r.ReadAtUtc
				}).ToList()
			})
			.ToListAsync();

		return Ok(new PagedResult<NotificationDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	[HttpGet("api/v1/admin/notifications/{id:long}")]
	public async Task<ActionResult<NotificationDto>> GetById(long id)
	{
		var item = await _db.Set<Notification>()
			.AsNoTracking()
			.Include(x => x.Recipients)
			.Where(x => x.NotificationId == id)
			.Select(x => new NotificationDto
			{
				NotificationId = x.NotificationId,
				NotificationTypeId = x.NotificationTypeId,
				Title = x.Title,
				Message = x.Message,
				RelatedTimetableEventId = x.RelatedTimetableEventId,
				RelatedRequestId = x.RelatedRequestId,
				IsActive = x.IsActive,
				Recipients = x.Recipients.Select(r => new NotificationRecipientDto
				{
					NotificationId = r.NotificationId,
					UserId = r.UserId,
					DeliveryStatus = r.DeliveryStatus,
					SentAtUtc = r.SentAtUtc,
					ReadAtUtc = r.ReadAtUtc
				}).ToList()
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	[HttpPost("api/v1/admin/notifications")]
	public async Task<ActionResult<NotificationDto>> Create([FromBody] NotificationCreateDto dto)
	{
		var typeExists = await _db.Set<NotificationType>().AnyAsync(x => x.NotificationTypeId == dto.NotificationTypeId);
		if (!typeExists) return NotFound($"NotificationType {dto.NotificationTypeId} not found.");

		if (dto.RelatedTimetableEventId.HasValue)
		{
			var eventExists = await _db.Set<TimetableEvent>().AnyAsync(x => x.TimetableEventId == dto.RelatedTimetableEventId.Value);
			if (!eventExists) return NotFound($"TimetableEvent {dto.RelatedTimetableEventId.Value} not found.");
		}

		if (dto.RelatedRequestId.HasValue)
		{
			var requestExists = await _db.Set<Request>().AnyAsync(x => x.RequestId == dto.RelatedRequestId.Value);
			if (!requestExists) return NotFound($"Request {dto.RelatedRequestId.Value} not found.");
		}

		var entity = new Notification
		{
			NotificationTypeId = dto.NotificationTypeId,
			Title = dto.Title,
			Message = dto.Message,
			RelatedTimetableEventId = dto.RelatedTimetableEventId,
			RelatedRequestId = dto.RelatedRequestId,
			IsActive = true,
			Recipients = dto.Recipients.Select(r => new NotificationRecipient
			{
				UserId = r.UserId,
				DeliveryStatus = r.DeliveryStatus,
				IsActive = true
			}).ToList()
		};

		_db.Add(entity);
		await _db.SaveChangesAsync();

		return CreatedAtAction(nameof(GetById), new { id = entity.NotificationId }, new NotificationDto
		{
			NotificationId = entity.NotificationId,
			NotificationTypeId = entity.NotificationTypeId,
			Title = entity.Title,
			Message = entity.Message,
			RelatedTimetableEventId = entity.RelatedTimetableEventId,
			RelatedRequestId = entity.RelatedRequestId,
			IsActive = entity.IsActive,
			Recipients = entity.Recipients.Select(r => new NotificationRecipientDto
			{
				NotificationId = r.NotificationId,
				UserId = r.UserId,
				DeliveryStatus = r.DeliveryStatus,
				SentAtUtc = r.SentAtUtc,
				ReadAtUtc = r.ReadAtUtc
			}).ToList()
		});
	}
}