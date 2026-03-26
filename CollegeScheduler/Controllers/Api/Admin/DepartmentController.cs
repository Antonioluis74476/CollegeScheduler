using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Academic;
using CollegeScheduler.DTOs.Academic;
using CollegeScheduler.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
public sealed class DepartmentController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public DepartmentController(ApplicationDbContext db)
	{
		_db = db;
	}

	// LIST (paged/filterable)
	[HttpGet("api/v1/admin/departments")]
	public async Task<ActionResult<PagedResult<DepartmentDto>>> GetAll([FromQuery] DepartmentQuery q)
	{
		var query = _db.Set<Department>().AsNoTracking();

		// Filtering
		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			var s = q.Search.Trim();
			query = query.Where(d =>
				d.Code.Contains(s) ||
				d.Name.Contains(s) ||
				(d.Email != null && d.Email.Contains(s)));
		}

		if (q.IsActive.HasValue)
			query = query.Where(d => d.IsActive == q.IsActive.Value);

		// Sorting
		var sortBy = (q.SortBy ?? "code").Trim().ToLowerInvariant();
		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

		query = sortBy switch
		{
			"name" => desc ? query.OrderByDescending(d => d.Name) : query.OrderBy(d => d.Name),
			_ => desc ? query.OrderByDescending(d => d.Code) : query.OrderBy(d => d.Code)
		};

		// Paging
		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(d => new DepartmentDto
			{
				DepartmentId = d.DepartmentId,
				Code = d.Code,
				Name = d.Name,
				Email = d.Email,
				IsActive = d.IsActive
			})
			.ToListAsync();

		return Ok(new PagedResult<DepartmentDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	// GET BY ID
	[HttpGet("api/v1/admin/departments/{id:int}")]
	public async Task<ActionResult<DepartmentDto>> GetById(int id)
	{
		var item = await _db.Set<Department>()
			.AsNoTracking()
			.Where(d => d.DepartmentId == id)
			.Select(d => new DepartmentDto
			{
				DepartmentId = d.DepartmentId,
				Code = d.Code,
				Name = d.Name,
				Email = d.Email,
				IsActive = d.IsActive
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	// CREATE
	[HttpPost("api/v1/admin/departments")]
	public async Task<ActionResult<DepartmentDto>> Create([FromBody] DepartmentCreateDto dto)
	{
		var code = dto.Code.Trim();

		if (await _db.Set<Department>().AnyAsync(d => d.Code == code))
			return Conflict($"Department code '{code}' already exists.");

		var entity = new Department
		{
			Code = code,
			Name = dto.Name.Trim(),
			Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
			IsActive = true
		};

		_db.Add(entity);
		await _db.SaveChangesAsync();

		return CreatedAtAction(nameof(GetById), new { id = entity.DepartmentId }, new DepartmentDto
		{
			DepartmentId = entity.DepartmentId,
			Code = entity.Code,
			Name = entity.Name,
			Email = entity.Email,
			IsActive = entity.IsActive
		});
	}

	// UPDATE
	[HttpPut("api/v1/admin/departments/{id:int}")]
	public async Task<IActionResult> Update(int id, [FromBody] DepartmentUpdateDto dto)
	{
		var entity = await _db.Set<Department>().FirstOrDefaultAsync(d => d.DepartmentId == id);
		if (entity is null) return NotFound();

		entity.Name = dto.Name.Trim();
		entity.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
		entity.IsActive = dto.IsActive;

		await _db.SaveChangesAsync();
		return NoContent();
	}
}
