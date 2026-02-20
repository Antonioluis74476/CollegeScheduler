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
public sealed class CohortModuleController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public CohortModuleController(ApplicationDbContext db)
	{
		_db = db;
	}

	// LIST plan for a cohort (nested)
	[HttpGet("api/v1/admin/cohorts/{cohortId:int}/modules")]
	public async Task<ActionResult<PagedResult<CohortModuleDto>>> GetForCohort(
		int cohortId,
		[FromQuery] CohortModuleQuery q)
	{
		var cohortExists = await _db.Set<Cohort>()
			.AsNoTracking()
			.AnyAsync(c => c.CohortId == cohortId);

		if (!cohortExists) return NotFound($"Cohort {cohortId} not found.");

		var query = _db.Set<CohortModule>()
			.AsNoTracking()
			.Where(x => x.CohortId == cohortId);

		// Filtering
		if (q.TermId.HasValue)
			query = query.Where(x => x.TermId == q.TermId.Value);

		if (q.ModuleId.HasValue)
			query = query.Where(x => x.ModuleId == q.ModuleId.Value);

		if (q.IsRequired.HasValue)
			query = query.Where(x => x.IsRequired == q.IsRequired.Value);

		if (q.IsActive.HasValue)
			query = query.Where(x => x.IsActive == q.IsActive.Value);

		// Search over module code/title (join)
		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			var s = q.Search.Trim();
			query = query.Where(x =>
				_db.Set<Module>().Any(m =>
					m.ModuleId == x.ModuleId &&
					(m.Code.Contains(s) || m.Title.Contains(s))));
		}

		// Sorting
		var sortBy = (q.SortBy ?? "moduleid").Trim().ToLowerInvariant();
		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

		query = sortBy switch
		{
			"termid" => desc ? query.OrderByDescending(x => x.TermId) : query.OrderBy(x => x.TermId),
			"isrequired" => desc ? query.OrderByDescending(x => x.IsRequired) : query.OrderBy(x => x.IsRequired),
			"createdat" => desc ? query.OrderByDescending(x => x.CreatedAtUtc) : query.OrderBy(x => x.CreatedAtUtc),
			_ => desc ? query.OrderByDescending(x => x.ModuleId) : query.OrderBy(x => x.ModuleId),
		};

		// Paging
		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new CohortModuleDto
			{
				CohortModuleId = x.CohortModuleId,
				CohortId = x.CohortId,
				ModuleId = x.ModuleId,
				TermId = x.TermId,
				IsRequired = x.IsRequired,
				IsActive = x.IsActive
			})
			.ToListAsync();

		return Ok(new PagedResult<CohortModuleDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	// GET by id (flat)
	[HttpGet("api/v1/admin/cohort-modules/{id:int}")]
	public async Task<ActionResult<CohortModuleDto>> GetById(int id)
	{
		var item = await _db.Set<CohortModule>()
			.AsNoTracking()
			.Where(x => x.CohortModuleId == id)
			.Select(x => new CohortModuleDto
			{
				CohortModuleId = x.CohortModuleId,
				CohortId = x.CohortId,
				ModuleId = x.ModuleId,
				TermId = x.TermId,
				IsRequired = x.IsRequired,
				IsActive = x.IsActive
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	// CREATE under cohort (nested)
	[HttpPost("api/v1/admin/cohorts/{cohortId:int}/modules")]
	public async Task<ActionResult<CohortModuleDto>> Create(int cohortId, [FromBody] CohortModuleCreateDto dto)
	{
		var cohortExists = await _db.Set<Cohort>().AnyAsync(c => c.CohortId == cohortId);
		if (!cohortExists) return NotFound($"Cohort {cohortId} not found.");

		var moduleExists = await _db.Set<Module>().AnyAsync(m => m.ModuleId == dto.ModuleId);
		if (!moduleExists) return NotFound($"Module {dto.ModuleId} not found.");

		var termExists = await _db.Set<Term>().AnyAsync(t => t.TermId == dto.TermId);
		if (!termExists) return NotFound($"Term {dto.TermId} not found.");

		var entity = new CohortModule
		{
			CohortId = cohortId,
			ModuleId = dto.ModuleId,
			TermId = dto.TermId,
			IsRequired = dto.IsRequired,
			IsActive = true
		};

		_db.Add(entity);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not save CohortModule. The combination (CohortId, ModuleId, TermId) must be unique.");
		}

		return CreatedAtAction(nameof(GetById), new { id = entity.CohortModuleId }, new CohortModuleDto
		{
			CohortModuleId = entity.CohortModuleId,
			CohortId = entity.CohortId,
			ModuleId = entity.ModuleId,
			TermId = entity.TermId,
			IsRequired = entity.IsRequired,
			IsActive = entity.IsActive
		});
	}

	// UPDATE
	[HttpPut("api/v1/admin/cohort-modules/{id:int}")]
	public async Task<IActionResult> Update(int id, [FromBody] CohortModuleUpdateDto dto)
	{
		var entity = await _db.Set<CohortModule>().FirstOrDefaultAsync(x => x.CohortModuleId == id);
		if (entity is null) return NotFound();

		var moduleExists = await _db.Set<Module>().AnyAsync(m => m.ModuleId == dto.ModuleId);
		if (!moduleExists) return NotFound($"Module {dto.ModuleId} not found.");

		var termExists = await _db.Set<Term>().AnyAsync(t => t.TermId == dto.TermId);
		if (!termExists) return NotFound($"Term {dto.TermId} not found.");

		entity.ModuleId = dto.ModuleId;
		entity.TermId = dto.TermId;
		entity.IsRequired = dto.IsRequired;
		entity.IsActive = dto.IsActive;

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not update CohortModule. The combination (CohortId, ModuleId, TermId) must be unique.");
		}

		return NoContent();
	}
}
