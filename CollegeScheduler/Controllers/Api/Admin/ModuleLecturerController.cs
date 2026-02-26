using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Academic;
using CollegeScheduler.Data.Entities.Profiles;
using CollegeScheduler.Data.Identity;
using CollegeScheduler.DTOs.Academic;
using CollegeScheduler.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
public sealed class ModuleLecturerController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public ModuleLecturerController(ApplicationDbContext db)
	{
		_db = db;
	}

	// LIST (flat)
	[HttpGet("api/v1/admin/module-lecturers")]
	public async Task<ActionResult<PagedResult<ModuleLecturerDto>>> GetAll([FromQuery] ModuleLecturerQuery q)
	{
		var query = _db.Set<ModuleLecturer>().AsNoTracking();

		// Filtering
		if (q.ModuleId.HasValue)
			query = query.Where(x => x.ModuleId == q.ModuleId.Value);

		if (q.LecturerId.HasValue)
			query = query.Where(x => x.LecturerId == q.LecturerId.Value);

		if (q.TermId.HasValue)
			query = query.Where(x => x.TermId == q.TermId.Value);

		if (!string.IsNullOrWhiteSpace(q.Role))
		{
			var role = q.Role.Trim();
			query = query.Where(x => x.Role == role);
		}

		// Sorting (deterministic)
		query = query
			.OrderBy(x => x.TermId)
			.ThenBy(x => x.ModuleId)
			.ThenBy(x => x.LecturerId);

		// Paging
		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new ModuleLecturerDto
			{
				ModuleId = x.ModuleId,
				LecturerId = x.LecturerId,
				TermId = x.TermId,
				Role = x.Role,
				AssignedAtUtc = x.AssignedAtUtc
			})
			.ToListAsync();

		return Ok(new PagedResult<ModuleLecturerDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	// GET by composite key
	[HttpGet("api/v1/admin/module-lecturers/{moduleId:int}/{lecturerId:int}/{termId:int}")]
	public async Task<ActionResult<ModuleLecturerDto>> GetById(int moduleId, int lecturerId, int termId)
	{
		var item = await _db.Set<ModuleLecturer>()
			.AsNoTracking()
			.Where(x =>
				x.ModuleId == moduleId &&
				x.LecturerId == lecturerId &&
				x.TermId == termId)
			.Select(x => new ModuleLecturerDto
			{
				ModuleId = x.ModuleId,
				LecturerId = x.LecturerId,
				TermId = x.TermId,
				Role = x.Role,
				AssignedAtUtc = x.AssignedAtUtc
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	// CREATE
	[HttpPost("api/v1/admin/module-lecturers")]
	public async Task<ActionResult<ModuleLecturerDto>> Create([FromBody] ModuleLecturerCreateDto dto)
	{
		var moduleExists = await _db.Set<Module>()
			.AnyAsync(m => m.ModuleId == dto.ModuleId);
		if (!moduleExists) return NotFound($"Module {dto.ModuleId} not found.");

		var lecturerExists = await _db.Set<LecturerProfile>()
			.AnyAsync(l => l.LecturerId == dto.LecturerId);
		if (!lecturerExists) return NotFound($"Lecturer {dto.LecturerId} not found.");

		var termExists = await _db.Set<Term>()
			.AnyAsync(t => t.TermId == dto.TermId);
		if (!termExists) return NotFound($"Term {dto.TermId} not found.");

		var entity = new ModuleLecturer
		{
			ModuleId = dto.ModuleId,
			LecturerId = dto.LecturerId,
			TermId = dto.TermId,
			Role = string.IsNullOrWhiteSpace(dto.Role) ? "Lead" : dto.Role.Trim(),
			AssignedAtUtc = DateTime.UtcNow
		};

		_db.Add(entity);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict(
				"Could not save ModuleLecturer. The combination (ModuleId, LecturerId, TermId) must be unique.");
		}

		return CreatedAtAction(
			nameof(GetById),
			new { moduleId = entity.ModuleId, lecturerId = entity.LecturerId, termId = entity.TermId },
			new ModuleLecturerDto
			{
				ModuleId = entity.ModuleId,
				LecturerId = entity.LecturerId,
				TermId = entity.TermId,
				Role = entity.Role,
				AssignedAtUtc = entity.AssignedAtUtc
			});
	}

	// UPDATE (role only)
	[HttpPut("api/v1/admin/module-lecturers/{moduleId:int}/{lecturerId:int}/{termId:int}")]
	public async Task<IActionResult> Update(
		int moduleId,
		int lecturerId,
		int termId,
		[FromBody] ModuleLecturerUpdateDto dto)
	{
		if (string.IsNullOrWhiteSpace(dto.Role))
			return BadRequest("Role is required.");

		var entity = await _db.Set<ModuleLecturer>()
			.FirstOrDefaultAsync(x =>
				x.ModuleId == moduleId &&
				x.LecturerId == lecturerId &&
				x.TermId == termId);

		if (entity is null) return NotFound();

		entity.Role = dto.Role.Trim();

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not update ModuleLecturer.");
		}

		return NoContent();
	}

	// DELETE
	[HttpDelete("api/v1/admin/module-lecturers/{moduleId:int}/{lecturerId:int}/{termId:int}")]
	public async Task<IActionResult> Delete(int moduleId, int lecturerId, int termId)
	{
		var entity = await _db.Set<ModuleLecturer>()
			.FirstOrDefaultAsync(x =>
				x.ModuleId == moduleId &&
				x.LecturerId == lecturerId &&
				x.TermId == termId);

		if (entity is null) return NotFound();

		_db.Remove(entity);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not delete ModuleLecturer.");
		}

		return NoContent();
	}
}