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
public sealed class NotificationTypeController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public NotificationTypeController(ApplicationDbContext db)
	{
		_db = db;
	}

	[HttpGet("api/v1/admin/notification-types")]
	public async Task<ActionResult<PagedResult<NotificationTypeDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
	{
		if (page < 1) page = 1;
		if (pageSize < 1) pageSize = 20;
		if (pageSize > 100) pageSize = 100;

		var query = _db.Set<NotificationType>().AsNoTracking().OrderBy(x => x.Name);

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new NotificationTypeDto
			{
				NotificationTypeId = x.NotificationTypeId,
				Name = x.Name,
				IsActive = x.IsActive
			})
			.ToListAsync();

		return Ok(new PagedResult<NotificationTypeDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	[HttpPost("api/v1/admin/notification-types")]
	public async Task<ActionResult<NotificationTypeDto>> Create([FromBody] NotificationTypeCreateDto dto)
	{
		var entity = new NotificationType
		{
			Name = dto.Name,
			IsActive = true
		};

		_db.Add(entity);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not save NotificationType. Name must be unique.");
		}

		return Ok(new NotificationTypeDto
		{
			NotificationTypeId = entity.NotificationTypeId,
			Name = entity.Name,
			IsActive = entity.IsActive
		});
	}
}