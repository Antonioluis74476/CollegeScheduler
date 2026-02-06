using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Facilities;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Facilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
public sealed class UnavailabilityReasonTypesController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public UnavailabilityReasonTypesController(ApplicationDbContext db)
	{
		_db = db;
	}

	[HttpGet("api/v1/admin/unavailability-reason-types")]
	public async Task<ActionResult<PagedResult<UnavailabilityReasonTypeDto>>> GetAll([FromQuery] UnavailabilityReasonTypeQuery q)
	{
		var query = _db.UnavailabilityReasonTypes.AsNoTracking();

		// Filtering
		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			var s = q.Search.Trim();
			query = query.Where(x => x.Name.Contains(s));
		}

		// Sorting
		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
		query = desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name);

		// Paging
		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new UnavailabilityReasonTypeDto
			{
				UnavailabilityReasonTypeId = x.UnavailabilityReasonTypeId,
				Name = x.Name
			})
			.ToListAsync();

		return Ok(new PagedResult<UnavailabilityReasonTypeDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	[HttpGet("api/v1/admin/unavailability-reason-types/{id:int}")]
	public async Task<ActionResult<UnavailabilityReasonTypeDto>> GetById(int id)
	{
		var item = await _db.UnavailabilityReasonTypes
			.AsNoTracking()
			.Where(x => x.UnavailabilityReasonTypeId == id)
			.Select(x => new UnavailabilityReasonTypeDto
			{
				UnavailabilityReasonTypeId = x.UnavailabilityReasonTypeId,
				Name = x.Name
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	[HttpPost("api/v1/admin/unavailability-reason-types")]
	public async Task<ActionResult<UnavailabilityReasonTypeDto>> Create([FromBody] UnavailabilityReasonTypeCreateDto dto)
	{
		var entity = new UnavailabilityReasonType
		{
			Name = dto.Name.Trim()
		};

		_db.UnavailabilityReasonTypes.Add(entity);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not save Reason Type. A reason with the same Name may already exist.");
		}

		var result = new UnavailabilityReasonTypeDto
		{
			UnavailabilityReasonTypeId = entity.UnavailabilityReasonTypeId,
			Name = entity.Name
		};

		return CreatedAtAction(nameof(GetById), new { id = entity.UnavailabilityReasonTypeId }, result);
	}

	[HttpPut("api/v1/admin/unavailability-reason-types/{id:int}")]
	public async Task<IActionResult> Update(int id, [FromBody] UnavailabilityReasonTypeUpdateDto dto)
	{
		var entity = await _db.UnavailabilityReasonTypes.FirstOrDefaultAsync(x => x.UnavailabilityReasonTypeId == id);
		if (entity is null) return NotFound();

		entity.Name = dto.Name.Trim();

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not update Reason Type. A reason with the same Name may already exist.");
		}

		return NoContent();
	}

	[HttpDelete("api/v1/admin/unavailability-reason-types/{id:int}")]
	public async Task<IActionResult> Delete(int id)
	{
		// Guard: if used by any RoomUnavailability, you probably want to block delete
		var isUsed = await _db.RoomUnavailabilities.AnyAsync(x => x.UnavailabilityReasonTypeId == id);
		if (isUsed) return Conflict("Cannot delete this Reason Type because it is used by Room Unavailabilities.");

		var entity = await _db.UnavailabilityReasonTypes.FindAsync(id);
		if (entity is null) return NotFound();

		_db.UnavailabilityReasonTypes.Remove(entity);
		await _db.SaveChangesAsync();

		return NoContent();
	}
}
