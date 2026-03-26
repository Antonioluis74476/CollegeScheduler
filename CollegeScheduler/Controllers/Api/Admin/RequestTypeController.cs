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
public sealed class RequestTypeController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public RequestTypeController(ApplicationDbContext db)
	{
		_db = db;
	}

	[HttpGet("api/v1/admin/request-types")]
	public async Task<ActionResult<PagedResult<RequestTypeDto>>> GetAll([FromQuery] RequestTypeQuery q)
	{
		var query = _db.Set<RequestType>().AsNoTracking().AsQueryable();

		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			var s = q.Search.Trim();
			query = query.Where(x => x.Name.Contains(s));
		}

		if (q.IsActive.HasValue)
			query = query.Where(x => x.IsActive == q.IsActive.Value);

		var sortBy = (q.SortBy ?? "name").Trim().ToLowerInvariant();
		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

		query = sortBy switch
		{
			"createdat" => desc ? query.OrderByDescending(x => x.CreatedAtUtc) : query.OrderBy(x => x.CreatedAtUtc),
			_ => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name)
		};

		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new RequestTypeDto
			{
				RequestTypeId = x.RequestTypeId,
				Name = x.Name,
				IsActive = x.IsActive
			})
			.ToListAsync();

		return Ok(new PagedResult<RequestTypeDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	[HttpGet("api/v1/admin/request-types/{id:int}")]
	public async Task<ActionResult<RequestTypeDto>> GetById(int id)
	{
		var item = await _db.Set<RequestType>()
			.AsNoTracking()
			.Where(x => x.RequestTypeId == id)
			.Select(x => new RequestTypeDto
			{
				RequestTypeId = x.RequestTypeId,
				Name = x.Name,
				IsActive = x.IsActive
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	[HttpPost("api/v1/admin/request-types")]
	public async Task<ActionResult<RequestTypeDto>> Create([FromBody] RequestTypeCreateDto dto)
	{
		var entity = new RequestType
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
			return Conflict("Could not save RequestType. Name must be unique.");
		}

		return CreatedAtAction(nameof(GetById), new { id = entity.RequestTypeId }, new RequestTypeDto
		{
			RequestTypeId = entity.RequestTypeId,
			Name = entity.Name,
			IsActive = entity.IsActive
		});
	}

	[HttpPut("api/v1/admin/request-types/{id:int}")]
	public async Task<IActionResult> Update(int id, [FromBody] RequestTypeUpdateDto dto)
	{
		var entity = await _db.Set<RequestType>().FirstOrDefaultAsync(x => x.RequestTypeId == id);
		if (entity is null) return NotFound();

		entity.Name = dto.Name;
		entity.IsActive = dto.IsActive;

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not update RequestType. Name must be unique.");
		}

		return NoContent();
	}
}