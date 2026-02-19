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
public sealed class AcademicYearController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public AcademicYearController(ApplicationDbContext db)
	{
		_db = db;
	}

	// LIST (paged/filter/search/sort)
	[HttpGet("api/v1/admin/academic-years")]
	public async Task<ActionResult<PagedResult<AcademicYearDto>>> GetAll([FromQuery] AcademicYearQuery q)
	{
		var query = _db.Set<AcademicYear>().AsNoTracking();

		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			var s = q.Search.Trim();
			query = query.Where(x => x.Label.Contains(s));
		}

		if (q.IsCurrent.HasValue)
			query = query.Where(x => x.IsCurrent == q.IsCurrent.Value);

		if (q.IsActive.HasValue)
			query = query.Where(x => x.IsActive == q.IsActive.Value);

		var sortBy = (q.SortBy ?? "label").Trim().ToLowerInvariant();
		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

		query = sortBy switch
		{
			"startdate" => desc ? query.OrderByDescending(x => x.StartDate) : query.OrderBy(x => x.StartDate),
			"enddate" => desc ? query.OrderByDescending(x => x.EndDate) : query.OrderBy(x => x.EndDate),
			"iscurrent" => desc ? query.OrderByDescending(x => x.IsCurrent) : query.OrderBy(x => x.IsCurrent),
			_ => desc ? query.OrderByDescending(x => x.Label) : query.OrderBy(x => x.Label),
		};

		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new AcademicYearDto
			{
				AcademicYearId = x.AcademicYearId,
				Label = x.Label,
				StartDate = x.StartDate,
				EndDate = x.EndDate,
				IsCurrent = x.IsCurrent,
				IsActive = x.IsActive
			})
			.ToListAsync();

		return Ok(new PagedResult<AcademicYearDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	// GET BY ID
	[HttpGet("api/v1/admin/academic-years/{id:int}")]
	public async Task<ActionResult<AcademicYearDto>> GetById(int id)
	{
		var item = await _db.Set<AcademicYear>()
			.AsNoTracking()
			.Where(x => x.AcademicYearId == id)
			.Select(x => new AcademicYearDto
			{
				AcademicYearId = x.AcademicYearId,
				Label = x.Label,
				StartDate = x.StartDate,
				EndDate = x.EndDate,
				IsCurrent = x.IsCurrent,
				IsActive = x.IsActive
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	// CREATE
	[HttpPost("api/v1/admin/academic-years")]
	public async Task<ActionResult<AcademicYearDto>> Create([FromBody] AcademicYearCreateDto dto)
	{
		var label = dto.Label.Trim();

		if (await _db.Set<AcademicYear>().AnyAsync(x => x.Label == label))
			return Conflict($"AcademicYear label '{label}' already exists.");

		var entity = new AcademicYear
		{
			Label = label,
			StartDate = dto.StartDate.Date,
			EndDate = dto.EndDate.Date,
			IsCurrent = dto.IsCurrent,
			IsActive = true
		};

		_db.Add(entity);

		// If making this year current, unset others (only one current)
		if (entity.IsCurrent)
		{
			var others = await _db.Set<AcademicYear>()
				.Where(x => x.IsCurrent && x.AcademicYearId != entity.AcademicYearId)
				.ToListAsync();

			foreach (var y in others)
				y.IsCurrent = false;
		}

		await _db.SaveChangesAsync();

		// If IsCurrent was requested, enforce "only one current" after we have the id
		if (entity.IsCurrent)
		{
			await _db.Set<AcademicYear>()
				.Where(x => x.AcademicYearId != entity.AcademicYearId && x.IsCurrent)
				.ExecuteUpdateAsync(s => s.SetProperty(x => x.IsCurrent, false));
		}

		return CreatedAtAction(nameof(GetById), new { id = entity.AcademicYearId }, new AcademicYearDto
		{
			AcademicYearId = entity.AcademicYearId,
			Label = entity.Label,
			StartDate = entity.StartDate,
			EndDate = entity.EndDate,
			IsCurrent = entity.IsCurrent,
			IsActive = entity.IsActive
		});
	}

	// UPDATE
	[HttpPut("api/v1/admin/academic-years/{id:int}")]
	public async Task<IActionResult> Update(int id, [FromBody] AcademicYearUpdateDto dto)
	{
		var entity = await _db.Set<AcademicYear>().FirstOrDefaultAsync(x => x.AcademicYearId == id);
		if (entity is null) return NotFound();

		entity.StartDate = dto.StartDate.Date;
		entity.EndDate = dto.EndDate.Date;
		entity.IsActive = dto.IsActive;

		// If setting current true -> unset others
		if (dto.IsCurrent && !entity.IsCurrent)
		{
			entity.IsCurrent = true;

			await _db.Set<AcademicYear>()
				.Where(x => x.AcademicYearId != id && x.IsCurrent)
				.ExecuteUpdateAsync(s => s.SetProperty(x => x.IsCurrent, false));
		}
		else
		{
			entity.IsCurrent = dto.IsCurrent;
		}

		await _db.SaveChangesAsync();
		return NoContent();
	}

	// SET CURRENT (explicit endpoint)
	[HttpPatch("api/v1/admin/academic-years/{id:int}/set-current")]
	public async Task<IActionResult> SetCurrent(int id)
	{
		var exists = await _db.Set<AcademicYear>().AnyAsync(x => x.AcademicYearId == id);
		if (!exists) return NotFound();

		await _db.Set<AcademicYear>()
			.Where(x => x.IsCurrent)
			.ExecuteUpdateAsync(s => s.SetProperty(x => x.IsCurrent, false));

		await _db.Set<AcademicYear>()
			.Where(x => x.AcademicYearId == id)
			.ExecuteUpdateAsync(s => s.SetProperty(x => x.IsCurrent, true));

		return NoContent();
	}
}
