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
public sealed class StudentCohortMembershipsController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public StudentCohortMembershipsController(ApplicationDbContext db)
	{
		_db = db;
	}

	// LIST (flat)
	[HttpGet("api/v1/admin/student-cohort-memberships")]
	public async Task<ActionResult<PagedResult<StudentCohortMembershipDto>>> GetAll([FromQuery] StudentCohortMembershipQuery q)
	{
		var query = _db.Set<StudentCohortMembership>().AsNoTracking();

		// Filtering
		if (q.StudentId.HasValue)
			query = query.Where(x => x.StudentId == q.StudentId.Value);

		if (q.CohortId.HasValue)
			query = query.Where(x => x.CohortId == q.CohortId.Value);

		if (q.AcademicYearId.HasValue)
			query = query.Where(x => x.AcademicYearId == q.AcademicYearId.Value);

		if (!string.IsNullOrWhiteSpace(q.MembershipType))
		{
			var type = q.MembershipType.Trim();
			query = query.Where(x => x.MembershipType == type);
		}

		// Sorting (simple + deterministic)
		query = query.OrderBy(x => x.StudentId).ThenBy(x => x.AcademicYearId).ThenBy(x => x.CohortId);

		// Paging
		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new StudentCohortMembershipDto
			{
				StudentId = x.StudentId,
				CohortId = x.CohortId,
				AcademicYearId = x.AcademicYearId,
				MembershipType = x.MembershipType,
				StartDate = x.StartDate,
				EndDate = x.EndDate
			})
			.ToListAsync();

		return Ok(new PagedResult<StudentCohortMembershipDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	// GET by composite key
	[HttpGet("api/v1/admin/student-cohort-memberships/{studentId:int}/{cohortId:int}/{academicYearId:int}")]
	public async Task<ActionResult<StudentCohortMembershipDto>> GetById(int studentId, int cohortId, int academicYearId)
	{
		var item = await _db.Set<StudentCohortMembership>()
			.AsNoTracking()
			.Where(x => x.StudentId == studentId && x.CohortId == cohortId && x.AcademicYearId == academicYearId)
			.Select(x => new StudentCohortMembershipDto
			{
				StudentId = x.StudentId,
				CohortId = x.CohortId,
				AcademicYearId = x.AcademicYearId,
				MembershipType = x.MembershipType,
				StartDate = x.StartDate,
				EndDate = x.EndDate
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	// LIST memberships for a student (nested)
	[HttpGet("api/v1/admin/students/{studentId:int}/cohort-memberships")]
	public async Task<ActionResult<PagedResult<StudentCohortMembershipDto>>> GetForStudent(int studentId, [FromQuery] StudentCohortMembershipQuery q)
	{
		var studentExists = await _db.Set<StudentProfile>()
			.AsNoTracking()
			.AnyAsync(s => s.StudentId == studentId);

		if (!studentExists) return NotFound($"Student {studentId} not found.");

		q.StudentId = studentId;
		return await GetAll(q);
	}

	// CREATE (flat)
	[HttpPost("api/v1/admin/student-cohort-memberships")]
	public async Task<ActionResult<StudentCohortMembershipDto>> Create([FromBody] StudentCohortMembershipCreateDto dto)
	{
		if (string.IsNullOrWhiteSpace(dto.MembershipType))
			return BadRequest("MembershipType is required.");

		var studentExists = await _db.Set<StudentProfile>().AnyAsync(s => s.StudentId == dto.StudentId);
		if (!studentExists) return NotFound($"Student {dto.StudentId} not found.");

		var cohortExists = await _db.Set<Cohort>().AnyAsync(c => c.CohortId == dto.CohortId);
		if (!cohortExists) return NotFound($"Cohort {dto.CohortId} not found.");

		var yearExists = await _db.Set<AcademicYear>().AnyAsync(y => y.AcademicYearId == dto.AcademicYearId);
		if (!yearExists) return NotFound($"AcademicYear {dto.AcademicYearId} not found.");

		var entity = new StudentCohortMembership
		{
			StudentId = dto.StudentId,
			CohortId = dto.CohortId,
			AcademicYearId = dto.AcademicYearId,
			MembershipType = dto.MembershipType.Trim(),
			StartDate = dto.StartDate,
			EndDate = dto.EndDate
		};

		_db.Add(entity);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not save StudentCohortMembership. The combination (StudentId, CohortId, AcademicYearId) must be unique.");
		}

		return CreatedAtAction(nameof(GetById),
			new { studentId = entity.StudentId, cohortId = entity.CohortId, academicYearId = entity.AcademicYearId },
			new StudentCohortMembershipDto
			{
				StudentId = entity.StudentId,
				CohortId = entity.CohortId,
				AcademicYearId = entity.AcademicYearId,
				MembershipType = entity.MembershipType,
				StartDate = entity.StartDate,
				EndDate = entity.EndDate
			});
	}

	// UPDATE (composite key)
	[HttpPut("api/v1/admin/student-cohort-memberships/{studentId:int}/{cohortId:int}/{academicYearId:int}")]
	public async Task<IActionResult> Update(int studentId, int cohortId, int academicYearId, [FromBody] StudentCohortMembershipUpdateDto dto)
	{
		if (string.IsNullOrWhiteSpace(dto.MembershipType))
			return BadRequest("MembershipType is required.");

		var entity = await _db.Set<StudentCohortMembership>()
			.FirstOrDefaultAsync(x => x.StudentId == studentId && x.CohortId == cohortId && x.AcademicYearId == academicYearId);

		if (entity is null) return NotFound();

		entity.MembershipType = dto.MembershipType.Trim();
		entity.StartDate = dto.StartDate;
		entity.EndDate = dto.EndDate;

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not update StudentCohortMembership.");
		}

		return NoContent();
	}

	// DELETE (composite key)
	[HttpDelete("api/v1/admin/student-cohort-memberships/{studentId:int}/{cohortId:int}/{academicYearId:int}")]
	public async Task<IActionResult> Delete(int studentId, int cohortId, int academicYearId)
	{
		var entity = await _db.Set<StudentCohortMembership>()
			.FirstOrDefaultAsync(x => x.StudentId == studentId && x.CohortId == cohortId && x.AcademicYearId == academicYearId);

		if (entity is null) return NotFound();

		_db.Remove(entity);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not delete StudentCohortMembership.");
		}

		return NoContent();
	}
}