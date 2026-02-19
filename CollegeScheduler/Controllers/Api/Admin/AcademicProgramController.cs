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
public sealed class AcademicProgramController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public AcademicProgramController(ApplicationDbContext db)
	{
		_db = db;
	}

	// LIST programs for a department (nested)
	[HttpGet("api/v1/admin/departments/{departmentId:int}/programs")]
	public async Task<ActionResult<PagedResult<ProgramDto>>> GetForDepartment(
		int departmentId,
		[FromQuery] ProgramQuery q)
	{
		var deptExists = await _db.Set<Department>()
			.AsNoTracking()
			.AnyAsync(d => d.DepartmentId == departmentId);

		if (!deptExists) return NotFound($"Department {departmentId} not found.");

		var query = _db.Set<AcademicProgram>()
			.AsNoTracking()
			.Where(p => p.DepartmentId == departmentId);

		// Filtering
		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			var s = q.Search.Trim();
			query = query.Where(p =>
				p.Code.Contains(s) ||
				p.Name.Contains(s) ||
				p.Level.Contains(s));
		}

		if (!string.IsNullOrWhiteSpace(q.Code))
		{
			var code = q.Code.Trim();
			query = query.Where(p => p.Code == code);
		}

		if (!string.IsNullOrWhiteSpace(q.Level))
		{
			var level = q.Level.Trim();
			query = query.Where(p => p.Level == level);
		}

		if (q.IsActive.HasValue)
			query = query.Where(p => p.IsActive == q.IsActive.Value);

		// Sorting (safe whitelist)
		var sortBy = (q.SortBy ?? "code").Trim().ToLowerInvariant();
		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

		query = sortBy switch
		{
			"name" => desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
			"level" => desc ? query.OrderByDescending(p => p.Level) : query.OrderBy(p => p.Level),
			"durationyears" => desc ? query.OrderByDescending(p => p.DurationYears) : query.OrderBy(p => p.DurationYears),
			_ => desc ? query.OrderByDescending(p => p.Code) : query.OrderBy(p => p.Code),
		};

		// Paging
		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(p => new ProgramDto
			{
				ProgramId = p.ProgramId,
				DepartmentId = p.DepartmentId,
				Code = p.Code,
				Name = p.Name,
				Level = p.Level,
				DurationYears = p.DurationYears,
				IsActive = p.IsActive
			})
			.ToListAsync();

		return Ok(new PagedResult<ProgramDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	// GET by id (flat)
	[HttpGet("api/v1/admin/programs/{id:int}")]
	public async Task<ActionResult<ProgramDto>> GetById(int id)
	{
		var item = await _db.Set<AcademicProgram>()
			.AsNoTracking()
			.Where(p => p.ProgramId == id)
			.Select(p => new ProgramDto
			{
				ProgramId = p.ProgramId,
				DepartmentId = p.DepartmentId,
				Code = p.Code,
				Name = p.Name,
				Level = p.Level,
				DurationYears = p.DurationYears,
				IsActive = p.IsActive
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	// CREATE under department (nested)
	[HttpPost("api/v1/admin/departments/{departmentId:int}/programs")]
	public async Task<ActionResult<ProgramDto>> Create(int departmentId, [FromBody] ProgramCreateDto dto)
	{
		var deptExists = await _db.Set<Department>()
			.AnyAsync(d => d.DepartmentId == departmentId);

		if (!deptExists) return NotFound($"Department {departmentId} not found.");

		var code = dto.Code.Trim();

		if (await _db.Set<AcademicProgram>().AnyAsync(p => p.Code == code))
			return Conflict($"Program code '{code}' already exists.");

		var entity = new AcademicProgram
		{
			DepartmentId = departmentId,
			Code = code,
			Name = dto.Name.Trim(),
			Level = dto.Level.Trim(),
			DurationYears = dto.DurationYears,
			IsActive = true
		};

		_db.Add(entity);
		await _db.SaveChangesAsync();

		return CreatedAtAction(nameof(GetById), new { id = entity.ProgramId }, new ProgramDto
		{
			ProgramId = entity.ProgramId,
			DepartmentId = entity.DepartmentId,
			Code = entity.Code,
			Name = entity.Name,
			Level = entity.Level,
			DurationYears = entity.DurationYears,
			IsActive = entity.IsActive
		});
	}

	// UPDATE
	[HttpPut("api/v1/admin/programs/{id:int}")]
	public async Task<IActionResult> Update(int id, [FromBody] ProgramUpdateDto dto)
	{
		var entity = await _db.Set<AcademicProgram>()
			.FirstOrDefaultAsync(p => p.ProgramId == id);

		if (entity is null) return NotFound();

		entity.Name = dto.Name.Trim();
		entity.Level = dto.Level.Trim();
		entity.DurationYears = dto.DurationYears;
		entity.IsActive = dto.IsActive;

		await _db.SaveChangesAsync();
		return NoContent();
	}
}
