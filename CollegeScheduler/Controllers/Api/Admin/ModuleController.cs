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
public sealed class ModuleController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public ModuleController(ApplicationDbContext db)
	{
		_db = db;
	}

	// LIST (paged/filter/search/sort)
	[HttpGet("api/v1/admin/modules")]
	public async Task<ActionResult<PagedResult<ModuleDto>>> GetAll([FromQuery] ModuleQuery q)
	{
		var query = _db.Set<Module>().AsNoTracking();

		// Filtering
		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			var s = q.Search.Trim();
			query = query.Where(m => m.Code.Contains(s) || m.Title.Contains(s));
		}

		if (!string.IsNullOrWhiteSpace(q.Code))
		{
			var code = q.Code.Trim();
			query = query.Where(m => m.Code == code);
		}

		if (q.DepartmentId.HasValue)
			query = query.Where(m => m.DepartmentId == q.DepartmentId.Value);

		if (q.IsActive.HasValue)
			query = query.Where(m => m.IsActive == q.IsActive.Value);

		if (q.MinCredits.HasValue)
			query = query.Where(m => m.Credits >= q.MinCredits.Value);

		if (q.MaxCredits.HasValue)
			query = query.Where(m => m.Credits <= q.MaxCredits.Value);

		if (q.MinRoomCapacity.HasValue)
			query = query.Where(m => m.MinRoomCapacity >= q.MinRoomCapacity.Value);

		// Sorting (safe whitelist)
		var sortBy = (q.SortBy ?? "code").Trim().ToLowerInvariant();
		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

		query = sortBy switch
		{
			"title" => desc ? query.OrderByDescending(m => m.Title) : query.OrderBy(m => m.Title),
			"credits" => desc ? query.OrderByDescending(m => m.Credits) : query.OrderBy(m => m.Credits),
			"minroomcapacity" => desc ? query.OrderByDescending(m => m.MinRoomCapacity) : query.OrderBy(m => m.MinRoomCapacity),
			_ => desc ? query.OrderByDescending(m => m.Code) : query.OrderBy(m => m.Code),
		};

		// Paging
		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(m => new ModuleDto
			{
				ModuleId = m.ModuleId,
				DepartmentId = m.DepartmentId,
				Code = m.Code,
				Title = m.Title,
				Credits = m.Credits,
				HoursPerWeek = m.HoursPerWeek,
				MinRoomCapacity = m.MinRoomCapacity,
				IsActive = m.IsActive
			})
			.ToListAsync();

		return Ok(new PagedResult<ModuleDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	// GET BY ID
	[HttpGet("api/v1/admin/modules/{id:int}")]
	public async Task<ActionResult<ModuleDto>> GetById(int id)
	{
		var item = await _db.Set<Module>()
			.AsNoTracking()
			.Where(m => m.ModuleId == id)
			.Select(m => new ModuleDto
			{
				ModuleId = m.ModuleId,
				DepartmentId = m.DepartmentId,
				Code = m.Code,
				Title = m.Title,
				Credits = m.Credits,
				HoursPerWeek = m.HoursPerWeek,
				MinRoomCapacity = m.MinRoomCapacity,
				IsActive = m.IsActive
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	// CREATE
	[HttpPost("api/v1/admin/modules")]
	public async Task<ActionResult<ModuleDto>> Create([FromBody] ModuleCreateDto dto)
	{
		var code = dto.Code.Trim();

		if (await _db.Set<Module>().AnyAsync(m => m.Code == code))
			return Conflict($"Module code '{code}' already exists.");

		if (dto.DepartmentId.HasValue)
		{
			var deptExists = await _db.Set<Department>()
				.AnyAsync(d => d.DepartmentId == dto.DepartmentId.Value);

			if (!deptExists) return NotFound($"Department {dto.DepartmentId.Value} not found.");
		}

		var entity = new Module
		{
			DepartmentId = dto.DepartmentId,
			Code = code,
			Title = dto.Title.Trim(),
			Credits = dto.Credits,
			HoursPerWeek = dto.HoursPerWeek,
			MinRoomCapacity = dto.MinRoomCapacity,
			IsActive = true
		};

		_db.Add(entity);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not save Module. Code must be unique.");
		}

		return CreatedAtAction(nameof(GetById), new { id = entity.ModuleId }, new ModuleDto
		{
			ModuleId = entity.ModuleId,
			DepartmentId = entity.DepartmentId,
			Code = entity.Code,
			Title = entity.Title,
			Credits = entity.Credits,
			HoursPerWeek = entity.HoursPerWeek,
			MinRoomCapacity = entity.MinRoomCapacity,
			IsActive = entity.IsActive
		});
	}

	// UPDATE
	[HttpPut("api/v1/admin/modules/{id:int}")]
	public async Task<IActionResult> Update(int id, [FromBody] ModuleUpdateDto dto)
	{
		var entity = await _db.Set<Module>().FirstOrDefaultAsync(m => m.ModuleId == id);
		if (entity is null) return NotFound();

		if (dto.DepartmentId.HasValue)
		{
			var deptExists = await _db.Set<Department>()
				.AnyAsync(d => d.DepartmentId == dto.DepartmentId.Value);

			if (!deptExists) return NotFound($"Department {dto.DepartmentId.Value} not found.");
		}

		entity.DepartmentId = dto.DepartmentId;
		entity.Code = dto.Code.Trim();
		entity.Title = dto.Title.Trim();
		entity.Credits = dto.Credits;
		entity.HoursPerWeek = dto.HoursPerWeek;
		entity.MinRoomCapacity = dto.MinRoomCapacity;
		entity.IsActive = dto.IsActive;

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not update Module. Code must be unique.");
		}

		return NoContent();
	}
}
