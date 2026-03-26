using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Academic;
using CollegeScheduler.Data.Entities.Membership;
using CollegeScheduler.Data.Entities.Profiles;
using CollegeScheduler.Data.Identity;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Membership;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
public sealed class StudentModuleEnrollmentsController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public StudentModuleEnrollmentsController(ApplicationDbContext db)
	{
		_db = db;
	}

	// LIST (flat)
	[HttpGet("api/v1/admin/student-module-enrollments")]
	public async Task<ActionResult<PagedResult<StudentModuleEnrollmentDto>>> GetAll([FromQuery] StudentModuleEnrollmentQuery q)
	{
		var query = _db.Set<StudentModuleEnrollment>().AsNoTracking();

		// Filtering
		if (q.StudentId.HasValue)
			query = query.Where(x => x.StudentId == q.StudentId.Value);

		if (q.ModuleId.HasValue)
			query = query.Where(x => x.ModuleId == q.ModuleId.Value);

		if (q.TermId.HasValue)
			query = query.Where(x => x.TermId == q.TermId.Value);

		if (q.AttendWithCohortId.HasValue)
			query = query.Where(x => x.AttendWithCohortId == q.AttendWithCohortId.Value);

		if (!string.IsNullOrWhiteSpace(q.Status))
		{
			var status = q.Status.Trim();
			query = query.Where(x => x.Status == status);
		}

		if (!string.IsNullOrWhiteSpace(q.EnrollmentType))
		{
			var type = q.EnrollmentType.Trim();
			query = query.Where(x => x.EnrollmentType == type);
		}

		// Sorting (most recent first)
		query = query.OrderByDescending(x => x.EnrolledAtUtc);

		// Paging
		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new StudentModuleEnrollmentDto
			{
				StudentId = x.StudentId,
				ModuleId = x.ModuleId,
				TermId = x.TermId,
				EnrollmentType = x.EnrollmentType,
				AttendWithCohortId = x.AttendWithCohortId,
				Status = x.Status,
				EnrolledAtUtc = x.EnrolledAtUtc
			})
			.ToListAsync();

		return Ok(new PagedResult<StudentModuleEnrollmentDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	// GET by composite key
	[HttpGet("api/v1/admin/student-module-enrollments/{studentId:int}/{moduleId:int}/{termId:int}")]
	public async Task<ActionResult<StudentModuleEnrollmentDto>> GetById(int studentId, int moduleId, int termId)
	{
		var item = await _db.Set<StudentModuleEnrollment>()
			.AsNoTracking()
			.Where(x => x.StudentId == studentId && x.ModuleId == moduleId && x.TermId == termId)
			.Select(x => new StudentModuleEnrollmentDto
			{
				StudentId = x.StudentId,
				ModuleId = x.ModuleId,
				TermId = x.TermId,
				EnrollmentType = x.EnrollmentType,
				AttendWithCohortId = x.AttendWithCohortId,
				Status = x.Status,
				EnrolledAtUtc = x.EnrolledAtUtc
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	// LIST enrollments for a student (nested)
	[HttpGet("api/v1/admin/students/{studentId:int}/module-enrollments")]
	public async Task<ActionResult<PagedResult<StudentModuleEnrollmentDto>>> GetForStudent(int studentId, [FromQuery] StudentModuleEnrollmentQuery q)
	{
		var studentExists = await _db.Set<StudentProfile>()
			.AsNoTracking()
			.AnyAsync(s => s.StudentId == studentId);

		if (!studentExists) return NotFound($"Student {studentId} not found.");

		q.StudentId = studentId;
		return await GetAll(q);
	}

	// CREATE (flat)
	[HttpPost("api/v1/admin/student-module-enrollments")]
	public async Task<ActionResult<StudentModuleEnrollmentDto>> Create([FromBody] StudentModuleEnrollmentCreateDto dto)
	{
		if (string.IsNullOrWhiteSpace(dto.EnrollmentType))
			return BadRequest("EnrollmentType is required.");

		var studentExists = await _db.Set<StudentProfile>().AnyAsync(s => s.StudentId == dto.StudentId);
		if (!studentExists) return NotFound($"Student {dto.StudentId} not found.");

		var moduleExists = await _db.Set<Module>().AnyAsync(m => m.ModuleId == dto.ModuleId);
		if (!moduleExists) return NotFound($"Module {dto.ModuleId} not found.");

		var termExists = await _db.Set<Term>().AnyAsync(t => t.TermId == dto.TermId);
		if (!termExists) return NotFound($"Term {dto.TermId} not found.");

		if (dto.AttendWithCohortId.HasValue)
		{
			var cohortExists = await _db.Set<Cohort>().AnyAsync(c => c.CohortId == dto.AttendWithCohortId.Value);
			if (!cohortExists) return NotFound($"Cohort {dto.AttendWithCohortId.Value} not found.");
		}

		var entity = new StudentModuleEnrollment
		{
			StudentId = dto.StudentId,
			ModuleId = dto.ModuleId,
			TermId = dto.TermId,
			EnrollmentType = dto.EnrollmentType.Trim(),
			AttendWithCohortId = dto.AttendWithCohortId,
			Status = string.IsNullOrWhiteSpace(dto.Status) ? "Enrolled" : dto.Status.Trim(),
			EnrolledAtUtc = DateTime.UtcNow
		};

		_db.Add(entity);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not save StudentModuleEnrollment. The combination (StudentId, ModuleId, TermId) must be unique.");
		}

		return CreatedAtAction(nameof(GetById),
			new { studentId = entity.StudentId, moduleId = entity.ModuleId, termId = entity.TermId },
			new StudentModuleEnrollmentDto
			{
				StudentId = entity.StudentId,
				ModuleId = entity.ModuleId,
				TermId = entity.TermId,
				EnrollmentType = entity.EnrollmentType,
				AttendWithCohortId = entity.AttendWithCohortId,
				Status = entity.Status,
				EnrolledAtUtc = entity.EnrolledAtUtc
			});
	}

	// UPDATE (composite key)
	[HttpPut("api/v1/admin/student-module-enrollments/{studentId:int}/{moduleId:int}/{termId:int}")]
	public async Task<IActionResult> Update(int studentId, int moduleId, int termId, [FromBody] StudentModuleEnrollmentUpdateDto dto)
	{
		if (string.IsNullOrWhiteSpace(dto.EnrollmentType))
			return BadRequest("EnrollmentType is required.");

		if (string.IsNullOrWhiteSpace(dto.Status))
			return BadRequest("Status is required.");

		var entity = await _db.Set<StudentModuleEnrollment>()
			.FirstOrDefaultAsync(x => x.StudentId == studentId && x.ModuleId == moduleId && x.TermId == termId);

		if (entity is null) return NotFound();

		if (dto.AttendWithCohortId.HasValue)
		{
			var cohortExists = await _db.Set<Cohort>().AnyAsync(c => c.CohortId == dto.AttendWithCohortId.Value);
			if (!cohortExists) return NotFound($"Cohort {dto.AttendWithCohortId.Value} not found.");
		}

		entity.EnrollmentType = dto.EnrollmentType.Trim();
		entity.AttendWithCohortId = dto.AttendWithCohortId;
		entity.Status = dto.Status.Trim();

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not update StudentModuleEnrollment.");
		}

		return NoContent();
	}

	// DELETE (composite key)
	[HttpDelete("api/v1/admin/student-module-enrollments/{studentId:int}/{moduleId:int}/{termId:int}")]
	public async Task<IActionResult> Delete(int studentId, int moduleId, int termId)
	{
		var entity = await _db.Set<StudentModuleEnrollment>()
			.FirstOrDefaultAsync(x => x.StudentId == studentId && x.ModuleId == moduleId && x.TermId == termId);

		if (entity is null) return NotFound();

		_db.Remove(entity);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not delete StudentModuleEnrollment.");
		}

		return NoContent();
	}
}