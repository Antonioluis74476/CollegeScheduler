using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Academic;
using CollegeScheduler.Data.Identity;
using CollegeScheduler.DTOs.Academic;
using CollegeScheduler.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
public sealed class TermController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public TermController(ApplicationDbContext db)
	{
		_db = db;
	}

	// LIST terms for an academic year (nested)
	[HttpGet("api/v1/admin/academic-years/{academicYearId:int}/terms")]
	public async Task<ActionResult<PagedResult<TermDto>>> GetForAcademicYear(
		int academicYearId,
		[FromQuery] TermQuery q)
	{
		var yearExists = await _db.Set<AcademicYear>()
			.AsNoTracking()
			.AnyAsync(y => y.AcademicYearId == academicYearId);

		if (!yearExists) return NotFound($"AcademicYear {academicYearId} not found.");

		var query = _db.Set<Term>()
			.AsNoTracking()
			.Where(t => t.AcademicYearId == academicYearId);

		// Filtering
		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			var s = q.Search.Trim();
			query = query.Where(t => t.Name.Contains(s));
		}

		if (q.TermNumber.HasValue)
			query = query.Where(t => t.TermNumber == q.TermNumber.Value);

		if (q.IsActive.HasValue)
			query = query.Where(t => t.IsActive == q.IsActive.Value);

		// Sorting
		var sortBy = (q.SortBy ?? "termNumber").Trim().ToLowerInvariant();
		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

		query = sortBy switch
		{
			"name" => desc ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
			"startdate" => desc ? query.OrderByDescending(t => t.StartDate) : query.OrderBy(t => t.StartDate),
			"enddate" => desc ? query.OrderByDescending(t => t.EndDate) : query.OrderBy(t => t.EndDate),
			_ => desc ? query.OrderByDescending(t => t.TermNumber) : query.OrderBy(t => t.TermNumber),
		};

		// Paging
		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(t => new TermDto
			{
				TermId = t.TermId,
				AcademicYearId = t.AcademicYearId,
				TermNumber = t.TermNumber,
				Name = t.Name,
				StartDate = t.StartDate,
				EndDate = t.EndDate,
				IsActive = t.IsActive
			})
			.ToListAsync();

		return Ok(new PagedResult<TermDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	// GET by id (flat)
	[HttpGet("api/v1/admin/terms/{id:int}")]
	public async Task<ActionResult<TermDto>> GetById(int id)
	{
		var item = await _db.Set<Term>()
			.AsNoTracking()
			.Where(t => t.TermId == id)
			.Select(t => new TermDto
			{
				TermId = t.TermId,
				AcademicYearId = t.AcademicYearId,
				TermNumber = t.TermNumber,
				Name = t.Name,
				StartDate = t.StartDate,
				EndDate = t.EndDate,
				IsActive = t.IsActive
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	// CREATE under academic year (nested)
	[HttpPost("api/v1/admin/academic-years/{academicYearId:int}/terms")]
	public async Task<ActionResult<TermDto>> Create(int academicYearId, [FromBody] TermCreateDto dto)
	{
		var yearExists = await _db.Set<AcademicYear>().AnyAsync(y => y.AcademicYearId == academicYearId);
		if (!yearExists) return NotFound($"AcademicYear {academicYearId} not found.");

		var entity = new Term
		{
			AcademicYearId = academicYearId,
			TermNumber = dto.TermNumber,
			Name = dto.Name.Trim(),
			StartDate = dto.StartDate.Date,
			EndDate = dto.EndDate.Date,
			IsActive = true
		};

		_db.Add(entity);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not save Term. TermNumber must be unique within the AcademicYear.");
		}

		return CreatedAtAction(nameof(GetById), new { id = entity.TermId }, new TermDto
		{
			TermId = entity.TermId,
			AcademicYearId = entity.AcademicYearId,
			TermNumber = entity.TermNumber,
			Name = entity.Name,
			StartDate = entity.StartDate,
			EndDate = entity.EndDate,
			IsActive = entity.IsActive
		});
	}

	// UPDATE
	[HttpPut("api/v1/admin/terms/{id:int}")]
	public async Task<IActionResult> Update(int id, [FromBody] TermUpdateDto dto)
	{
		var entity = await _db.Set<Term>().FirstOrDefaultAsync(t => t.TermId == id);
		if (entity is null) return NotFound();

		entity.TermNumber = dto.TermNumber;
		entity.Name = dto.Name.Trim();
		entity.StartDate = dto.StartDate.Date;
		entity.EndDate = dto.EndDate.Date;
		entity.IsActive = dto.IsActive;

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not update Term. TermNumber must be unique within the AcademicYear.");
		}

		return NoContent();
	}
}
