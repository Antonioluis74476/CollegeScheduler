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
public sealed class StudentAccountsController : ControllerBase
{
	private readonly ApplicationDbContext _db;
	private readonly UserManager<ApplicationUser> _userManager;

	public StudentAccountsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
	{
		_db = db;
		_userManager = userManager;
	}

	// LIST (paged/filter/search/sort) 
	[HttpGet("api/v1/admin/students")]
	public async Task<ActionResult<PagedResult<StudentDto>>> GetAll([FromQuery] StudentQuery q)
	{
		var query = _db.StudentProfiles.AsNoTracking();

		// Filtering
		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			var s = q.Search.Trim();
			query = query.Where(p =>
				p.StudentNumber.Contains(s) ||
				p.Name.Contains(s) ||
				p.LastName.Contains(s) ||
				p.Email.Contains(s));
		}

		if (!string.IsNullOrWhiteSpace(q.StudentNumber))
		{
			var sn = q.StudentNumber.Trim();
			query = query.Where(p => p.StudentNumber == sn);
		}

		if (!string.IsNullOrWhiteSpace(q.Status))
		{
			var st = q.Status.Trim();
			query = query.Where(p => p.Status == st);
		}

		if (q.IsActive.HasValue)
			query = query.Where(p => p.IsActive == q.IsActive.Value);

		// Sorting (safe whitelist)
		var sortBy = (q.SortBy ?? "studentNumber").Trim().ToLowerInvariant();
		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

		query = sortBy switch
		{
			"name" => desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
			"lastname" => desc ? query.OrderByDescending(p => p.LastName) : query.OrderBy(p => p.LastName),
			"email" => desc ? query.OrderByDescending(p => p.Email) : query.OrderBy(p => p.Email),
			"status" => desc ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status),
			"createdat" => desc ? query.OrderByDescending(p => p.CreatedAtUtc) : query.OrderBy(p => p.CreatedAtUtc),
			_ => desc ? query.OrderByDescending(p => p.StudentNumber) : query.OrderBy(p => p.StudentNumber),
		};

		// Paging
		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(p => new StudentDto
			{
				StudentId = p.StudentId,
				UserId = p.UserId ?? "",
				Email = p.Email,
				StudentNumber = p.StudentNumber,
				Name = p.Name,
				LastName = p.LastName,
				Status = p.Status,
				IsActive = p.IsActive,
				CreatedAtUtc = p.CreatedAtUtc,
				UpdatedAtUtc = p.UpdatedAtUtc
			})
			.ToListAsync();

		return Ok(new PagedResult<StudentDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	// GET BY ID
	[HttpGet("api/v1/admin/students/{id:int}")]
	public async Task<ActionResult<StudentDto>> GetById(int id)
	{
		var item = await _db.StudentProfiles
			.AsNoTracking()
			.Where(p => p.StudentId == id)
			.Select(p => new StudentDto
			{
				StudentId = p.StudentId,
				UserId = p.UserId ?? "",
				Email = p.Email,
				StudentNumber = p.StudentNumber,
				Name = p.Name,
				LastName = p.LastName,
				Status = p.Status,
				IsActive = p.IsActive,
				CreatedAtUtc = p.CreatedAtUtc,
				UpdatedAtUtc = p.UpdatedAtUtc
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	// CREATE (Identity user + Student role + StudentProfile)
	[HttpPost("api/v1/admin/students")]
	public async Task<ActionResult<StudentDto>> Create([FromBody] StudentCreateDto dto)
	{
		var email = dto.Email.Trim();
		var studentNumber = dto.StudentNumber.Trim();

		// Validate uniqueness in Profiles
		if (await _db.StudentProfiles.AnyAsync(p => p.StudentNumber == studentNumber))
			return Conflict($"StudentNumber '{studentNumber}' already exists.");

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

		// 2) Assign Student role
		var addRole = await _userManager.AddToRoleAsync(user, RoleNames.Student);
		if (!addRole.Succeeded)
			return BadRequest(addRole.Errors.Select(e => e.Description));

		// 3) Create Profile linked to user.Id
		var profile = new StudentProfile
		{
			UserId = user.Id,
			StudentNumber = studentNumber,
			Name = dto.Name.Trim(),
			LastName = dto.LastName.Trim(),
			Email = email,
			Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim(),
			IsActive = true
		};

		_db.StudentProfiles.Add(profile);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not save StudentProfile. StudentNumber or UserId might already be in use.");
		}

		var result = new StudentDto
		{
			StudentId = profile.StudentId,
			UserId = user.Id,
			Email = profile.Email,
			StudentNumber = profile.StudentNumber,
			Name = profile.Name,
			LastName = profile.LastName,
			Status = profile.Status,
			IsActive = profile.IsActive,
			CreatedAtUtc = profile.CreatedAtUtc,
			UpdatedAtUtc = profile.UpdatedAtUtc
		};

		return CreatedAtAction(nameof(GetById), new { id = profile.StudentId }, result);
	}

	// UPDATE (profile fields only)
	[HttpPut("api/v1/admin/students/{id:int}")]
	public async Task<IActionResult> Update(int id, [FromBody] StudentUpdateDto dto)
	{
		var p = await _db.StudentProfiles.FirstOrDefaultAsync(x => x.StudentId == id);
		if (p is null) return NotFound();

		p.Name = dto.Name.Trim();
		p.LastName = dto.LastName.Trim();
		p.Status = string.IsNullOrWhiteSpace(dto.Status) ? p.Status : dto.Status.Trim();
		p.IsActive = dto.IsActive;

		await _db.SaveChangesAsync();
		return NoContent();
	}

	// ACTIVATE / DEACTIVATE (RoomController style toggle endpoints)
	[HttpPatch("api/v1/admin/students/{id:int}/deactivate")]
	public async Task<IActionResult> Deactivate(int id)
	{
		var p = await _db.StudentProfiles.FirstOrDefaultAsync(x => x.StudentId == id);
		if (p is null) return NotFound();

		p.IsActive = false;
		await _db.SaveChangesAsync();
		return NoContent();
	}

	[HttpPatch("api/v1/admin/students/{id:int}/activate")]
	public async Task<IActionResult> Activate(int id)
	{
		var p = await _db.StudentProfiles.FirstOrDefaultAsync(x => x.StudentId == id);
		if (p is null) return NotFound();

		p.IsActive = true;
		await _db.SaveChangesAsync();
		return NoContent();
	}

	// RESET PASSWORD (admin)
	[HttpPost("api/v1/admin/students/{id:int}/reset-password")]
	public async Task<IActionResult> ResetPassword(int id, [FromBody] PasswordResetDto dto)
	{
		var p = await _db.StudentProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.StudentId == id);
		if (p is null) return NotFound();

		if (string.IsNullOrWhiteSpace(p.UserId))
			return BadRequest("This student profile is not linked to an Identity user.");

		var user = await _userManager.FindByIdAsync(p.UserId);
		if (user is null) return NotFound("Linked user not found.");

		var token = await _userManager.GeneratePasswordResetTokenAsync(user);
		var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

		return result.Succeeded
			? NoContent()
			: BadRequest(result.Errors.Select(e => e.Description));
	}

	// DEBUG: show roles (proves AspNetUserRoles is correct)
	[HttpGet("api/v1/admin/students/{id:int}/roles")]
	public async Task<ActionResult<string[]>> GetRoles(int id)
	{
		var p = await _db.StudentProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.StudentId == id);
		if (p is null) return NotFound();

		if (string.IsNullOrWhiteSpace(p.UserId))
			return BadRequest("This student profile is not linked to an Identity user.");

		var user = await _userManager.FindByIdAsync(p.UserId);
		if (user is null) return NotFound("Linked user not found.");

		var roles = await _userManager.GetRolesAsync(user);
		return Ok(roles.ToArray());
	}
}
