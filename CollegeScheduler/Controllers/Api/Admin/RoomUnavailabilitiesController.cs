using System.Security.Claims;
using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Facilities;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Facilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
public sealed class RoomUnavailabilitiesController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public RoomUnavailabilitiesController(ApplicationDbContext db)
	{
		_db = db;
	}

	// List unavailabilities for a room (paged + filterable)
	[HttpGet("api/v1/admin/rooms/{roomId:int}/unavailabilities")]
	public async Task<ActionResult<PagedResult<RoomUnavailabilityDto>>> GetForRoom(int roomId, [FromQuery] RoomUnavailabilityQuery q)
	{
		var roomExists = await _db.Rooms.AsNoTracking().AnyAsync(r => r.RoomId == roomId);
		if (!roomExists) return NotFound($"Room {roomId} not found.");

		var query = _db.RoomUnavailabilities
			.AsNoTracking()
			.Include(x => x.UnavailabilityReasonType)
			.Where(x => x.RoomId == roomId);

		// Filterss
		if (q.FromUtc.HasValue)
			query = query.Where(x => x.EndUtc > q.FromUtc.Value);

		if (q.ToUtc.HasValue)
			query = query.Where(x => x.StartUtc < q.ToUtc.Value);

		if (q.ReasonTypeId.HasValue)
			query = query.Where(x => x.UnavailabilityReasonTypeId == q.ReasonTypeId.Value);

		// Sorting
		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
		var sortBy = (q.SortBy ?? "start").Trim().ToLowerInvariant();

		query = sortBy switch
		{
			"end" => desc ? query.OrderByDescending(x => x.EndUtc) : query.OrderBy(x => x.EndUtc),
			"created" => desc ? query.OrderByDescending(x => x.CreatedAtUtc) : query.OrderBy(x => x.CreatedAtUtc),
			_ => desc ? query.OrderByDescending(x => x.StartUtc) : query.OrderBy(x => x.StartUtc)
		};

		// Paging
		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new RoomUnavailabilityDto
			{
				RoomUnavailabilityId = x.RoomUnavailabilityId,
				RoomId = x.RoomId,
				StartUtc = x.StartUtc,
				EndUtc = x.EndUtc,
				UnavailabilityReasonTypeId = x.UnavailabilityReasonTypeId,
				ReasonName = x.UnavailabilityReasonType != null ? x.UnavailabilityReasonType.Name : null,
				Notes = x.Notes,
				CreatedByUserId = x.CreatedByUserId,
				CreatedAtUtc = x.CreatedAtUtc,
				UpdatedAtUtc = x.UpdatedAtUtc
			})
			.ToListAsync();

		return Ok(new PagedResult<RoomUnavailabilityDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	// Get single unavailability by id
	[HttpGet("api/v1/admin/room-unavailabilities/{id:int}")]
	public async Task<ActionResult<RoomUnavailabilityDto>> GetById(int id)
	{
		var item = await _db.RoomUnavailabilities
			.AsNoTracking()
			.Include(x => x.UnavailabilityReasonType)
			.Where(x => x.RoomUnavailabilityId == id)
			.Select(x => new RoomUnavailabilityDto
			{
				RoomUnavailabilityId = x.RoomUnavailabilityId,
				RoomId = x.RoomId,
				StartUtc = x.StartUtc,
				EndUtc = x.EndUtc,
				UnavailabilityReasonTypeId = x.UnavailabilityReasonTypeId,
				ReasonName = x.UnavailabilityReasonType != null ? x.UnavailabilityReasonType.Name : null,
				Notes = x.Notes,
				CreatedByUserId = x.CreatedByUserId,
				CreatedAtUtc = x.CreatedAtUtc,
				UpdatedAtUtc = x.UpdatedAtUtc
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	// Create unavailability for a room
	[HttpPost("api/v1/admin/rooms/{roomId:int}/unavailabilities")]
	public async Task<ActionResult<RoomUnavailabilityDto>> Create(int roomId, [FromBody] RoomUnavailabilityCreateDto dto)
	{
		if (dto.EndUtc <= dto.StartUtc)
			return BadRequest("EndUtc must be after StartUtc.");

		var roomExists = await _db.Rooms.AnyAsync(r => r.RoomId == roomId);
		if (!roomExists) return NotFound($"Room {roomId} not found.");

		var reasonExists = await _db.UnavailabilityReasonTypes.AnyAsync(t => t.UnavailabilityReasonTypeId == dto.UnavailabilityReasonTypeId);
		if (!reasonExists) return NotFound($"ReasonType {dto.UnavailabilityReasonTypeId} not found.");

		// Prevent overlaps (intersection check)
		var overlaps = await _db.RoomUnavailabilities.AnyAsync(x =>
			x.RoomId == roomId &&
			x.StartUtc < dto.EndUtc &&
			x.EndUtc > dto.StartUtc);

		if (overlaps)
			return Conflict("This time range overlaps an existing unavailability for this room.");

		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrWhiteSpace(userId))
			return Unauthorized("No user id claim found.");

		var entity = new RoomUnavailability
		{
			RoomId = roomId,
			StartUtc = dto.StartUtc,
			EndUtc = dto.EndUtc,
			UnavailabilityReasonTypeId = dto.UnavailabilityReasonTypeId,
			Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
			CreatedByUserId = userId
		};

		_db.RoomUnavailabilities.Add(entity);
		await _db.SaveChangesAsync();

		// Return created
		var created = await _db.RoomUnavailabilities
			.AsNoTracking()
			.Include(x => x.UnavailabilityReasonType)
			.Where(x => x.RoomUnavailabilityId == entity.RoomUnavailabilityId)
			.Select(x => new RoomUnavailabilityDto
			{
				RoomUnavailabilityId = x.RoomUnavailabilityId,
				RoomId = x.RoomId,
				StartUtc = x.StartUtc,
				EndUtc = x.EndUtc,
				UnavailabilityReasonTypeId = x.UnavailabilityReasonTypeId,
				ReasonName = x.UnavailabilityReasonType != null ? x.UnavailabilityReasonType.Name : null,
				Notes = x.Notes,
				CreatedByUserId = x.CreatedByUserId,
				CreatedAtUtc = x.CreatedAtUtc,
				UpdatedAtUtc = x.UpdatedAtUtc
			})
			.FirstAsync();

		return CreatedAtAction(nameof(GetById), new { id = created.RoomUnavailabilityId }, created);
	}

	// Update unavailability
	[HttpPut("api/v1/admin/room-unavailabilities/{id:int}")]
	public async Task<IActionResult> Update(int id, [FromBody] RoomUnavailabilityUpdateDto dto)
	{
		if (dto.EndUtc <= dto.StartUtc)
			return BadRequest("EndUtc must be after StartUtc.");

		var entity = await _db.RoomUnavailabilities.FirstOrDefaultAsync(x => x.RoomUnavailabilityId == id);
		if (entity is null) return NotFound();

		var reasonExists = await _db.UnavailabilityReasonTypes.AnyAsync(t => t.UnavailabilityReasonTypeId == dto.UnavailabilityReasonTypeId);
		if (!reasonExists) return NotFound($"ReasonType {dto.UnavailabilityReasonTypeId} not found.");

		// Prevent overlaps (exclude itself)
		var overlaps = await _db.RoomUnavailabilities.AnyAsync(x =>
			x.RoomId == entity.RoomId &&
			x.RoomUnavailabilityId != id &&
			x.StartUtc < dto.EndUtc &&
			x.EndUtc > dto.StartUtc);

		if (overlaps)
			return Conflict("This time range overlaps an existing unavailability for this room.");

		entity.StartUtc = dto.StartUtc;
		entity.EndUtc = dto.EndUtc;
		entity.UnavailabilityReasonTypeId = dto.UnavailabilityReasonTypeId;
		entity.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

		await _db.SaveChangesAsync();
		return NoContent();
	}

	// Delete
	[HttpDelete("api/v1/admin/room-unavailabilities/{id:int}")]
	public async Task<IActionResult> Delete(int id)
	{
		var entity = await _db.RoomUnavailabilities.FindAsync(id);
		if (entity is null) return NotFound();

		_db.RoomUnavailabilities.Remove(entity);
		await _db.SaveChangesAsync();

		return NoContent();
	}
}