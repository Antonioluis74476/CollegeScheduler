using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Facilities;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Facilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Route("api/v1/admin/features")]
[Authorize(Roles = "Admin")]
public sealed class FeatureController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public FeatureController(ApplicationDbContext db)
	{
		_db = db;
	}

	[HttpGet]
	public async Task<ActionResult<PagedResult<FeatureDto>>> GetAll([FromQuery] FeatureQuery q)
	{
		var query = _db.Features.AsNoTracking();

		// Filtering
		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			var s = q.Search.Trim();
			query = query.Where(f => f.Name.Contains(s));
		}

		// Sorting
		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
		query = desc ? query.OrderByDescending(f => f.Name) : query.OrderBy(f => f.Name);

		// Paging
		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(f => new FeatureDto
			{
				FeatureId = f.FeatureId,
				Name = f.Name
			})
			.ToListAsync();

		return Ok(new PagedResult<FeatureDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	[HttpGet("{id:int}")]
	public async Task<ActionResult<FeatureDto>> GetById(int id)
	{
		var item = await _db.Features
			.AsNoTracking()
			.Where(f => f.FeatureId == id)
			.Select(f => new FeatureDto
			{
				FeatureId = f.FeatureId,
				Name = f.Name
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	[HttpPost]
	public async Task<ActionResult<FeatureDto>> Create([FromBody] FeatureCreateDto dto)
	{
		var feature = new Feature
		{
			Name = dto.Name.Trim()
		};

		_db.Features.Add(feature);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not save Feature. A feature with the same Name may already exist.");
		}

		var result = new FeatureDto
		{
			FeatureId = feature.FeatureId,
			Name = feature.Name
		};

		return CreatedAtAction(nameof(GetById), new { id = feature.FeatureId }, result);
	}

	[HttpPut("{id:int}")]
	public async Task<IActionResult> Update(int id, [FromBody] FeatureUpdateDto dto)
	{
		var feature = await _db.Features.FirstOrDefaultAsync(f => f.FeatureId == id);
		if (feature is null) return NotFound();

		feature.Name = dto.Name.Trim();

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not update Feature. A feature with the same Name may already exist.");
		}

		return NoContent();
	}

	[HttpDelete("{id:int}")]
	public async Task<IActionResult> Delete(int id)
	{
		var feature = await _db.Features.FindAsync(id);
		if (feature is null) return NotFound();

		_db.Features.Remove(feature);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			// If RoomFeature rows exist, FK may block delete depending on your config.
			return Conflict("Cannot delete Feature because it is in use by one or more rooms.");
		}

		return NoContent();
	}
}
