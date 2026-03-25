using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Audit;
using CollegeScheduler.Data.Identity;
using CollegeScheduler.DTOs.Audit;
using CollegeScheduler.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
public sealed class AuditLogController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public AuditLogController(ApplicationDbContext db)
	{
		_db = db;
	}

	[HttpGet("api/v1/admin/audit-logs")]
	public async Task<ActionResult<PagedResult<AuditLogDto>>> GetAll([FromQuery] AuditLogQuery q)
	{
		var query = _db.Set<AuditLog>().AsNoTracking().AsQueryable();

		if (!string.IsNullOrWhiteSpace(q.UserId))
			query = query.Where(x => x.UserId == q.UserId);

		if (!string.IsNullOrWhiteSpace(q.Action))
			query = query.Where(x => x.Action == q.Action);

		if (!string.IsNullOrWhiteSpace(q.EntityType))
			query = query.Where(x => x.EntityType == q.EntityType);

		if (!string.IsNullOrWhiteSpace(q.EntityId))
			query = query.Where(x => x.EntityId == q.EntityId);

		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
		query = desc ? query.OrderByDescending(x => x.PerformedAtUtc) : query.OrderBy(x => x.PerformedAtUtc);

		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new AuditLogDto
			{
				AuditLogId = x.AuditLogId,
				UserId = x.UserId,
				Action = x.Action,
				EntityType = x.EntityType,
				EntityId = x.EntityId,
				OldValuesJson = x.OldValuesJson,
				NewValuesJson = x.NewValuesJson,
				IpAddress = x.IpAddress,
				UserAgent = x.UserAgent,
				PerformedAtUtc = x.PerformedAtUtc
			})
			.ToListAsync();

		return Ok(new PagedResult<AuditLogDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}
}