using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Profiles;
using CollegeScheduler.Data.Identity;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
public sealed class LecturerAccountsController : ControllerBase
{
	private readonly ApplicationDbContext _db;
	private readonly UserManager<ApplicationUser> _userManager;

	public LecturerAccountsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
	{
		_db = db;
		_userManager = userManager;
	}

	// LIST (paged/filter/search/sort) 
	[HttpGet("api/v1/admin/lecturers")]
	public async Task<ActionResult<PagedResult<LecturerDto>>> GetAll([FromQuery] LecturerQuery q)
	{
		var query = _db.LecturerProfiles.AsNoTracking();

		// Filtering
		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			var s = q.Search.Trim();
			query = query.Where(p =>
				p.StaffNumber.Contains(s) ||
				p.Name.Contains(s) ||
				p.LastName.Contains(s) ||
				p.Email.Contains(s) ||
				p.EmploymentType.Contains(s));
		}

		if (!string.IsNullOrWhiteSpace(q.StaffNumber))
		{
			var sn = q.StaffNumber.Trim();
			query = query.Where(p => p.StaffNumber == sn);
		}

		if (q.DepartmentId.HasValue)
			query = query.Where(p => p.DepartmentId == q.DepartmentId.Value);

		if (!string.IsNullOrWhiteSpace(q.EmploymentType))
		{
			var et = q.EmploymentType.Trim();
			query = query.Where(p => p.EmploymentType == et);
		}

		if (q.IsActive.HasValue)
			query = query.Where(p => p.IsActive == q.IsActive.Value);

		// Sorting (safe whitelist)
		var sortBy = (q.SortBy ?? "staffNumber").Trim().ToLowerInvariant();
		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

		query = sortBy switch
		{
			"name" => desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
			"lastname" => desc ? query.OrderByDescending(p => p.LastName) : query.OrderBy(p => p.LastName),
			"email" => desc ? query.OrderByDescending(p => p.Email) : query.OrderBy(p => p.Email),
			"departmentid" => desc ? query.OrderByDescending(p => p.DepartmentId) : query.OrderBy(p => p.DepartmentId),
			"employmenttype" => desc ? query.OrderByDescending(p => p.EmploymentType) : query.OrderBy(p => p.EmploymentType),
			"createdat" => desc ? query.OrderByDescending(p => p.CreatedAtUtc) : query.OrderBy(p => p.CreatedAtUtc),
			_ => desc ? query.OrderByDescending(p => p.StaffNumber) : query.OrderBy(p => p.StaffNumber),
		};

		// Paging
		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(p => new LecturerDto
			{
				LecturerId = p.LecturerId,
				UserId = p.UserId ?? "",
				Email = p.Email,
				StaffNumber = p.StaffNumber,
				Name = p.Name,
				LastName = p.LastName,
				DepartmentId = p.DepartmentId,
				EmploymentType = p.EmploymentType,
				MaxWeeklyHours = p.MaxWeeklyHours,
				IsActive = p.IsActive,
				CreatedAtUtc = p.CreatedAtUtc,
				UpdatedAtUtc = p.UpdatedAtUtc
			})
			.ToListAsync();

		return Ok(new PagedResult<LecturerDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	// GET BY ID
	[HttpGet("api/v1/admin/lecturers/{id:int}")]
	public async Task<ActionResult<LecturerDto>> GetById(int id)
	{
		var item = await _db.LecturerProfiles
			.AsNoTracking()
			.Where(p => p.LecturerId == id)
			.Select(p => new LecturerDto
			{
				LecturerId = p.LecturerId,
				UserId = p.UserId ?? "",
				Email = p.Email,
				StaffNumber = p.StaffNumber,
				Name = p.Name,
				LastName = p.LastName,
				DepartmentId = p.DepartmentId,
				EmploymentType = p.EmploymentType,
				MaxWeeklyHours = p.MaxWeeklyHours,
				IsActive = p.IsActive,
				CreatedAtUtc = p.CreatedAtUtc,
				UpdatedAtUtc = p.UpdatedAtUtc
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	// CREATE (Identity user + Lecturer role + LecturerProfile)
	[HttpPost("api/v1/admin/lecturers")]
	public async Task<ActionResult<LecturerDto>> Create([FromBody] LecturerCreateDto dto)
	{
		var email = dto.Email.Trim();
		var staffNumber = dto.StaffNumber.Trim();

		// Validate uniqueness in Profiles
		if (await _db.LecturerProfiles.AnyAsync(p => p.StaffNumber == staffNumber))
			return Conflict($"StaffNumber '{staffNumber}' already exists.");

		// Validate uniqueness in Identity
		var existingUser = await _userManager.FindByEmailAsync(email);
		if (existingUser != null)
			return Conflict($"User with email '{email}' already exists.");

		// 1) Create Identity user
		var user = new ApplicationUser
		{
			UserName = email,
			Email = email,
			EmailConfirmed = true
		};

		var createUser = await _userManager.CreateAsync(user, dto.Password);
		if (!createUser.Succeeded)
			return BadRequest(createUser.Errors.Select(e => e.Description));

		// 2) Assign Lecturer role
		var addRole = await _userManager.AddToRoleAsync(user, RoleNames.Lecturer);
		if (!addRole.Succeeded)
			return BadRequest(addRole.Errors.Select(e => e.Description));

		// 3) Create Profile linked to user.Id
		var profile = new LecturerProfile
		{
			UserId = user.Id,
			StaffNumber = staffNumber,
			Name = dto.Name.Trim(),
			LastName = dto.LastName.Trim(),
			Email = email,
			DepartmentId = dto.DepartmentId,
			EmploymentType = string.IsNullOrWhiteSpace(dto.EmploymentType) ? "FullTime" : dto.EmploymentType.Trim(),
			MaxWeeklyHours = dto.MaxWeeklyHours,
			IsActive = true
		};

		_db.LecturerProfiles.Add(profile);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not save LecturerProfile. StaffNumber or UserId might already be in use.");
		}

		var result = new LecturerDto
		{
			LecturerId = profile.LecturerId,
			UserId = user.Id,
			Email = profile.Email,
			StaffNumber = profile.StaffNumber,
			Name = profile.Name,
			LastName = profile.LastName,
			DepartmentId = profile.DepartmentId,
			EmploymentType = profile.EmploymentType,
			MaxWeeklyHours = profile.MaxWeeklyHours,
			IsActive = profile.IsActive,
			CreatedAtUtc = profile.CreatedAtUtc,
			UpdatedAtUtc = profile.UpdatedAtUtc
		};

		return CreatedAtAction(nameof(GetById), new { id = profile.LecturerId }, result);
	}

	// UPDATE (profile fields only)
	[HttpPut("api/v1/admin/lecturers/{id:int}")]
	public async Task<IActionResult> Update(int id, [FromBody] LecturerUpdateDto dto)
	{
		var p = await _db.LecturerProfiles.FirstOrDefaultAsync(x => x.LecturerId == id);
		if (p is null) return NotFound();

		p.Name = dto.Name.Trim();
		p.LastName = dto.LastName.Trim();
		p.DepartmentId = dto.DepartmentId;
		p.EmploymentType = string.IsNullOrWhiteSpace(dto.EmploymentType) ? p.EmploymentType : dto.EmploymentType.Trim();
		p.MaxWeeklyHours = dto.MaxWeeklyHours;
		p.IsActive = dto.IsActive;

		await _db.SaveChangesAsync();
		return NoContent();
	}

	// ACTIVATE / DEACTIVATE
	[HttpPatch("api/v1/admin/lecturers/{id:int}/deactivate")]
	public async Task<IActionResult> Deactivate(int id)
	{
		var p = await _db.LecturerProfiles.FirstOrDefaultAsync(x => x.LecturerId == id);
		if (p is null) return NotFound();

		p.IsActive = false;
		await _db.SaveChangesAsync();
		return NoContent();
	}

	[HttpPatch("api/v1/admin/lecturers/{id:int}/activate")]
	public async Task<IActionResult> Activate(int id)
	{
		var p = await _db.LecturerProfiles.FirstOrDefaultAsync(x => x.LecturerId == id);
		if (p is null) return NotFound();

		p.IsActive = true;
		await _db.SaveChangesAsync();
		return NoContent();
	}

	// RESET PASSWORD (admin)
	[HttpPost("api/v1/admin/lecturers/{id:int}/reset-password")]
	public async Task<IActionResult> ResetPassword(int id, [FromBody] PasswordResetDto dto)
	{
		var p = await _db.LecturerProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.LecturerId == id);
		if (p is null) return NotFound();

		if (string.IsNullOrWhiteSpace(p.UserId))
			return BadRequest("This lecturer profile is not linked to an Identity user.");

		var user = await _userManager.FindByIdAsync(p.UserId);
		if (user is null) return NotFound("Linked user not found.");

		var token = await _userManager.GeneratePasswordResetTokenAsync(user);
		var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

		return result.Succeeded
			? NoContent()
			: BadRequest(result.Errors.Select(e => e.Description));
	}

	// DEBUG: show roles (proves AspNetUserRoles is correct)
	[HttpGet("api/v1/admin/lecturers/{id:int}/roles")]
	public async Task<ActionResult<string[]>> GetRoles(int id)
	{
		var p = await _db.LecturerProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.LecturerId == id);
		if (p is null) return NotFound();

		if (string.IsNullOrWhiteSpace(p.UserId))
			return BadRequest("This lecturer profile is not linked to an Identity user.");

		var user = await _userManager.FindByIdAsync(p.UserId);
		if (user is null) return NotFound("Linked user not found.");

		var roles = await _userManager.GetRolesAsync(user);
		return Ok(roles.ToArray());
	}
}
