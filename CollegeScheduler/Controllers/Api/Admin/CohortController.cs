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
public sealed class CohortController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public CohortController(ApplicationDbContext db)
	{
		_db = db;
	}

	// LIST cohorts for a program (nested)
	[HttpGet("api/v1/admin/programs/{programId:int}/cohorts")]
	public async Task<ActionResult<PagedResult<CohortDto>>> GetForProgram(
		int programId,
		[FromQuery] CohortQuery q)
	{
		var programExists = await _db.Set<AcademicProgram>()
			.AsNoTracking()
			.AnyAsync(p => p.ProgramId == programId);

		if (!programExists) return NotFound($"Program {programId} not found.");

		var query = _db.Set<Cohort>()
			.AsNoTracking()
			.Where(c => c.ProgramId == programId);

		// Filtering
		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			var s = q.Search.Trim();
			query = query.Where(c => c.Code.Contains(s) || c.Name.Contains(s));
		}

		if (q.AcademicYearId.HasValue)
			query = query.Where(c => c.AcademicYearId == q.AcademicYearId.Value);

		if (q.YearOfStudy.HasValue)
			query = query.Where(c => c.YearOfStudy == q.YearOfStudy.Value);

		if (!string.IsNullOrWhiteSpace(q.Code))
		{
			var code = q.Code.Trim();
			query = query.Where(c => c.Code == code);
		}

		if (q.IsActive.HasValue)
			query = query.Where(c => c.IsActive == q.IsActive.Value);

		// Sorting
		var sortBy = (q.SortBy ?? "code").Trim().ToLowerInvariant();
		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

		query = sortBy switch
		{
			"name" => desc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
			"yearofstudy" => desc ? query.OrderByDescending(c => c.YearOfStudy) : query.OrderBy(c => c.YearOfStudy),
			"expectedsize" => desc ? query.OrderByDescending(c => c.ExpectedSize) : query.OrderBy(c => c.ExpectedSize),
			"academicyearid" => desc ? query.OrderByDescending(c => c.AcademicYearId) : query.OrderBy(c => c.AcademicYearId),
			_ => desc ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
		};

		// Paging
		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(c => new CohortDto
			{
				CohortId = c.CohortId,
				ProgramId = c.ProgramId,
				AcademicYearId = c.AcademicYearId,
				YearOfStudy = c.YearOfStudy,
				Code = c.Code,
				Name = c.Name,
				ExpectedSize = c.ExpectedSize,
				IsActive = c.IsActive
			})
			.ToListAsync();

		return Ok(new PagedResult<CohortDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	// GET by id (flat)
	[HttpGet("api/v1/admin/cohorts/{id:int}")]
	public async Task<ActionResult<CohortDto>> GetById(int id)
	{
		var item = await _db.Set<Cohort>()
			.AsNoTracking()
			.Where(c => c.CohortId == id)
			.Select(c => new CohortDto
			{
				CohortId = c.CohortId,
				ProgramId = c.ProgramId,
				AcademicYearId = c.AcademicYearId,
				YearOfStudy = c.YearOfStudy,
				Code = c.Code,
				Name = c.Name,
				ExpectedSize = c.ExpectedSize,
				IsActive = c.IsActive
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	// CREATE under program (nested)
	[HttpPost("api/v1/admin/programs/{programId:int}/cohorts")]
	public async Task<ActionResult<CohortDto>> Create(int programId, [FromBody] CohortCreateDto dto)
	{
		var programExists = await _db.Set<AcademicProgram>().AnyAsync(p => p.ProgramId == programId);
		if (!programExists) return NotFound($"Program {programId} not found.");

		var yearExists = await _db.Set<AcademicYear>().AnyAsync(y => y.AcademicYearId == dto.AcademicYearId);
		if (!yearExists) return NotFound($"AcademicYear {dto.AcademicYearId} not found.");

		var entity = new Cohort
		{
			ProgramId = programId,
			AcademicYearId = dto.AcademicYearId,
			YearOfStudy = dto.YearOfStudy,
			Code = dto.Code.Trim(),
			Name = dto.Name.Trim(),
			ExpectedSize = dto.ExpectedSize,
			IsActive = true
		};

		_db.Add(entity);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not save Cohort. The combination (ProgramId, AcademicYearId, YearOfStudy, Code) must be unique.");
		}

		return CreatedAtAction(nameof(GetById), new { id = entity.CohortId }, new CohortDto
		{
			CohortId = entity.CohortId,
			ProgramId = entity.ProgramId,
			AcademicYearId = entity.AcademicYearId,
			YearOfStudy = entity.YearOfStudy,
			Code = entity.Code,
			Name = entity.Name,
			ExpectedSize = entity.ExpectedSize,
			IsActive = entity.IsActive
		});
	}

	// UPDATE
	[HttpPut("api/v1/admin/cohorts/{id:int}")]
	public async Task<IActionResult> Update(int id, [FromBody] CohortUpdateDto dto)
	{
		var entity = await _db.Set<Cohort>().FirstOrDefaultAsync(c => c.CohortId == id);
		if (entity is null) return NotFound();

		var yearExists = await _db.Set<AcademicYear>().AnyAsync(y => y.AcademicYearId == dto.AcademicYearId);
		if (!yearExists) return NotFound($"AcademicYear {dto.AcademicYearId} not found.");

		entity.AcademicYearId = dto.AcademicYearId;
		entity.YearOfStudy = dto.YearOfStudy;
		entity.Code = dto.Code.Trim();
		entity.Name = dto.Name.Trim();
		entity.ExpectedSize = dto.ExpectedSize;
		entity.IsActive = dto.IsActive;

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not update Cohort. The combination (ProgramId, AcademicYearId, YearOfStudy, Code) must be unique.");
		}

		return NoContent();
	}
}
