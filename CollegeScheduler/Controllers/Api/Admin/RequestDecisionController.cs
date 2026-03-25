using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Requests;
using CollegeScheduler.Data.Identity;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
public sealed class RequestDecisionController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public RequestDecisionController(ApplicationDbContext db)
	{
		_db = db;
	}

	[HttpGet("api/v1/admin/request-decisions")]
	public async Task<ActionResult<PagedResult<RequestDecisionDto>>> GetAll([FromQuery] RequestDecisionQuery q)
	{
		var query = _db.Set<RequestDecision>().AsNoTracking().AsQueryable();

		if (q.RequestId.HasValue)
			query = query.Where(x => x.RequestId == q.RequestId.Value);

		if (!string.IsNullOrWhiteSpace(q.DecidedByUserId))
			query = query.Where(x => x.DecidedByUserId == q.DecidedByUserId);

		if (!string.IsNullOrWhiteSpace(q.Decision))
			query = query.Where(x => x.Decision == q.Decision);

		if (q.IsActive.HasValue)
			query = query.Where(x => x.IsActive == q.IsActive.Value);

		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
		query = desc ? query.OrderByDescending(x => x.DecidedAtUtc) : query.OrderBy(x => x.DecidedAtUtc);

		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new RequestDecisionDto
			{
				RequestDecisionId = x.RequestDecisionId,
				RequestId = x.RequestId,
				DecidedByUserId = x.DecidedByUserId,
				Decision = x.Decision,
				Comment = x.Comment,
				DecidedAtUtc = x.DecidedAtUtc,
				IsActive = x.IsActive
			})
			.ToListAsync();

		return Ok(new PagedResult<RequestDecisionDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	[HttpPost("api/v1/admin/requests/{requestId:long}/decisions")]
	public async Task<ActionResult<RequestDecisionDto>> Create(long requestId, [FromBody] RequestDecisionCreateDto dto)
	{
		var request = await _db.Set<Request>().FirstOrDefaultAsync(x => x.RequestId == requestId);
		if (request is null) return NotFound($"Request {requestId} not found.");

		var userExists = await _db.Users.AnyAsync(x => x.Id == dto.DecidedByUserId);
		if (!userExists) return NotFound($"User {dto.DecidedByUserId} not found.");

		var normalized = dto.Decision.Trim();
		if (normalized != "Approved" && normalized != "Rejected")
			return BadRequest("Decision must be Approved or Rejected.");

		var entity = new RequestDecision
		{
			RequestId = requestId,
			DecidedByUserId = dto.DecidedByUserId,
			Decision = normalized,
			Comment = dto.Comment,
			DecidedAtUtc = DateTime.UtcNow,
			IsActive = true
		};

		_db.Add(entity);

		// optionally update Request status automatically
		var approvedStatus = await _db.Set<RequestStatus>().FirstOrDefaultAsync(x => x.Name == "Approved");
		var rejectedStatus = await _db.Set<RequestStatus>().FirstOrDefaultAsync(x => x.Name == "Rejected");

		if (normalized == "Approved" && approvedStatus is not null)
			request.RequestStatusId = approvedStatus.RequestStatusId;
		else if (normalized == "Rejected" && rejectedStatus is not null)
			request.RequestStatusId = rejectedStatus.RequestStatusId;

		await _db.SaveChangesAsync();

		return Ok(new RequestDecisionDto
		{
			RequestDecisionId = entity.RequestDecisionId,
			RequestId = entity.RequestId,
			DecidedByUserId = entity.DecidedByUserId,
			Decision = entity.Decision,
			Comment = entity.Comment,
			DecidedAtUtc = entity.DecidedAtUtc,
			IsActive = entity.IsActive
		});
	}
}