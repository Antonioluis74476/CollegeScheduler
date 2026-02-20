using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Facilities;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Facilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public sealed class BuildingController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public BuildingController(ApplicationDbContext db)
	{
		_db = db;
	}

	// List buildings for a campus (paged/filterable)
	[HttpGet("campuses/{campusId:int}/buildings")]
	public async Task<ActionResult<PagedResult<BuildingDto>>> GetForCampus(
		int campusId,
		[FromQuery] BuildingQuery q)
	{
		var campusExists = await _db.Campuses.AsNoTracking().AnyAsync(c => c.CampusId == campusId);
		if (!campusExists) return NotFound($"Campus {campusId} not found.");

		var query = _db.Buildings
			.AsNoTracking()
			.Where(b => b.CampusId == campusId);

		// Filtering
		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			var s = q.Search.Trim();
			query = query.Where(b =>
				b.Name.Contains(s) ||
				b.Code.Contains(s) ||
				(b.Faculty != null && b.Faculty.Contains(s)));
		}

		if (!string.IsNullOrWhiteSpace(q.Code))
		{
			var code = q.Code.Trim();
			query = query.Where(b => b.Code == code);
		}

		if (!string.IsNullOrWhiteSpace(q.Faculty))
		{
			var faculty = q.Faculty.Trim();
			query = query.Where(b => b.Faculty != null && b.Faculty.Contains(faculty));
		}

		if (q.IsActive.HasValue)
		{
			query = query.Where(b => b.IsActive == q.IsActive.Value);
		}

		// Sorting
		var sortBy = (q.SortBy ?? "name").Trim().ToLowerInvariant();
		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

		query = sortBy switch
		{
			"code" => desc ? query.OrderByDescending(b => b.Code) : query.OrderBy(b => b.Code),
			"faculty" => desc ? query.OrderByDescending(b => b.Faculty) : query.OrderBy(b => b.Faculty),
			_ => desc ? query.OrderByDescending(b => b.Name) : query.OrderBy(b => b.Name)
		};

		// Paging
		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(b => new BuildingDto
			{
				BuildingId = b.BuildingId,
				CampusId = b.CampusId,
				Code = b.Code,
				Name = b.Name,
				Faculty = b.Faculty,
				IsActive = b.IsActive
			})
			.ToListAsync();

		return Ok(new PagedResult<BuildingDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	// Get building by id
	[HttpGet("buildings/{id:int}")]
	public async Task<ActionResult<BuildingDto>> GetById(int id)
	{
		var item = await _db.Buildings
			.AsNoTracking()
			.Where(b => b.BuildingId == id)
			.Select(b => new BuildingDto
			{
				BuildingId = b.BuildingId,
				CampusId = b.CampusId,
				Code = b.Code,
				Name = b.Name,
				Faculty = b.Faculty,
				IsActive = b.IsActive
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	// Create building under a campus
	[HttpPost("campuses/{campusId:int}/buildings")]
	public async Task<ActionResult<BuildingDto>> Create(int campusId, [FromBody] BuildingCreateDto dto)
	{
		var campusExists = await _db.Campuses.AsNoTracking().AnyAsync(c => c.CampusId == campusId);
		if (!campusExists) return NotFound($"Campus {campusId} not found.");

		var building = new Building
		{
			CampusId = campusId,
			Code = dto.Code.Trim(),
			Name = dto.Name.Trim(),
			Faculty = string.IsNullOrWhiteSpace(dto.Faculty) ? null : dto.Faculty.Trim(),
			IsActive = true
		};

		_db.Buildings.Add(building);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not save Building. A building with the same Code may already exist in this campus.");
		}

		var result = new BuildingDto
		{
			BuildingId = building.BuildingId,
			CampusId = building.CampusId,
			Code = building.Code,
			Name = building.Name,
			Faculty = building.Faculty,
			IsActive = building.IsActive
		};

		return CreatedAtAction(nameof(GetById), new { id = building.BuildingId }, result);
	}

	// Update building
	[HttpPut("buildings/{id:int}")]
	public async Task<IActionResult> Update(int id, [FromBody] BuildingUpdateDto dto)
	{
		var building = await _db.Buildings.FirstOrDefaultAsync(b => b.BuildingId == id);
		if (building is null) return NotFound();

		building.Code = dto.Code.Trim();
		building.Name = dto.Name.Trim();
		building.Faculty = string.IsNullOrWhiteSpace(dto.Faculty) ? null : dto.Faculty.Trim();
		building.IsActive = dto.IsActive;

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not update Building. A building with the same Code may already exist in this campus.");
		}

		return NoContent();
	}

	// Soft delete (recommended)
	[HttpDelete("buildings/{id:int}")]
	public async Task<IActionResult> Delete(int id)
	{
		var building = await _db.Buildings.FirstOrDefaultAsync(b => b.BuildingId == id);
		if (building is null) return NotFound();

		building.IsActive = false;
		await _db.SaveChangesAsync();

		return NoContent();
	}
}
