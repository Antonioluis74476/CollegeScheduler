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
public sealed class RoomController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public RoomController(ApplicationDbContext db)
	{
		_db = db;
	}

	// List rooms for a building (paged/filterable)
	[HttpGet("api/v1/admin/buildings/{buildingId:int}/rooms")]
	public async Task<ActionResult<PagedResult<RoomDto>>> GetForBuilding(
		int buildingId,
		[FromQuery] RoomQuery q)
	{
		var buildingExists = await _db.Buildings.AsNoTracking().AnyAsync(b => b.BuildingId == buildingId);
		if (!buildingExists) return NotFound($"Building {buildingId} not found.");

		var query = _db.Rooms
			.AsNoTracking()
			.Where(r => r.BuildingId == buildingId);

		// Filtering
		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			var s = q.Search.Trim();
			query = query.Where(r =>
				r.Code.Contains(s) ||
				(r.Name != null && r.Name.Contains(s)) ||
				(r.Floor != null && r.Floor.Contains(s)));
		}

		if (!string.IsNullOrWhiteSpace(q.Code))
		{
			var code = q.Code.Trim();
			query = query.Where(r => r.Code == code);
		}

		if (q.RoomTypeId.HasValue)
			query = query.Where(r => r.RoomTypeId == q.RoomTypeId.Value);

		if (q.IsActive.HasValue)
			query = query.Where(r => r.IsActive == q.IsActive.Value);

		if (q.IsBookableByStudents.HasValue)
			query = query.Where(r => r.IsBookableByStudents == q.IsBookableByStudents.Value);

		if (q.RequiresApproval.HasValue)
			query = query.Where(r => r.RequiresApproval == q.RequiresApproval.Value);

		if (q.MinCapacity.HasValue)
			query = query.Where(r => r.Capacity >= q.MinCapacity.Value);

		// Sorting (safe)
		var sortBy = (q.SortBy ?? "code").Trim().ToLowerInvariant();
		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

		query = sortBy switch
		{
			"name" => desc ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
			"capacity" => desc ? query.OrderByDescending(r => r.Capacity) : query.OrderBy(r => r.Capacity),
			_ => desc ? query.OrderByDescending(r => r.Code) : query.OrderBy(r => r.Code),
		};

		// Paging
		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(r => new RoomDto
			{
				RoomId = r.RoomId,
				BuildingId = r.BuildingId,
				RoomTypeId = r.RoomTypeId,
				Code = r.Code,
				Name = r.Name,
				Floor = r.Floor,
				Capacity = r.Capacity,
				IsBookableByStudents = r.IsBookableByStudents,
				RequiresApproval = r.RequiresApproval,
				Notes = r.Notes,
				IsActive = r.IsActive
			})
			.ToListAsync();

		return Ok(new PagedResult<RoomDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	// Get room by id
	[HttpGet("api/v1/admin/rooms/{id:int}")]
	public async Task<ActionResult<RoomDto>> GetById(int id)
	{
		var item = await _db.Rooms
			.AsNoTracking()
			.Where(r => r.RoomId == id)
			.Select(r => new RoomDto
			{
				RoomId = r.RoomId,
				BuildingId = r.BuildingId,
				RoomTypeId = r.RoomTypeId,
				Code = r.Code,
				Name = r.Name,
				Floor = r.Floor,
				Capacity = r.Capacity,
				IsBookableByStudents = r.IsBookableByStudents,
				RequiresApproval = r.RequiresApproval,
				Notes = r.Notes,
				IsActive = r.IsActive
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	// Create room under a building
	[HttpPost("api/v1/admin/buildings/{buildingId:int}/rooms")]
	public async Task<ActionResult<RoomDto>> Create(int buildingId, [FromBody] RoomCreateDto dto)
	{
		var buildingExists = await _db.Buildings.AnyAsync(b => b.BuildingId == buildingId);
		if (!buildingExists) return NotFound($"Building {buildingId} not found.");

		var roomTypeExists = await _db.RoomTypes.AnyAsync(rt => rt.RoomTypeId == dto.RoomTypeId);
		if (!roomTypeExists) return NotFound($"RoomType {dto.RoomTypeId} not found.");

		var room = new Room
		{
			BuildingId = buildingId,
			RoomTypeId = dto.RoomTypeId,
			Code = dto.Code.Trim(),
			Name = string.IsNullOrWhiteSpace(dto.Name) ? null : dto.Name.Trim(),
			Floor = string.IsNullOrWhiteSpace(dto.Floor) ? null : dto.Floor.Trim(),
			Capacity = dto.Capacity,
			IsBookableByStudents = dto.IsBookableByStudents,
			RequiresApproval = dto.RequiresApproval,
			Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
			IsActive = true
		};

		_db.Rooms.Add(room);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not save Room. A room with the same Code may already exist in this building.");
		}

		var result = new RoomDto
		{
			RoomId = room.RoomId,
			BuildingId = room.BuildingId,
			RoomTypeId = room.RoomTypeId,
			Code = room.Code,
			Name = room.Name,
			Floor = room.Floor,
			Capacity = room.Capacity,
			IsBookableByStudents = room.IsBookableByStudents,
			RequiresApproval = room.RequiresApproval,
			Notes = room.Notes,
			IsActive = room.IsActive
		};

		return CreatedAtAction(nameof(GetById), new { id = room.RoomId }, result);
	}

	// Update room
	[HttpPut("api/v1/admin/rooms/{id:int}")]
	public async Task<IActionResult> Update(int id, [FromBody] RoomUpdateDto dto)
	{
		var room = await _db.Rooms.FirstOrDefaultAsync(r => r.RoomId == id);
		if (room is null) return NotFound();

		var roomTypeExists = await _db.RoomTypes.AnyAsync(rt => rt.RoomTypeId == dto.RoomTypeId);
		if (!roomTypeExists) return NotFound($"RoomType {dto.RoomTypeId} not found.");

		room.RoomTypeId = dto.RoomTypeId;
		room.Code = dto.Code.Trim();
		room.Name = string.IsNullOrWhiteSpace(dto.Name) ? null : dto.Name.Trim();
		room.Floor = string.IsNullOrWhiteSpace(dto.Floor) ? null : dto.Floor.Trim();
		room.Capacity = dto.Capacity;
		room.IsBookableByStudents = dto.IsBookableByStudents;
		room.RequiresApproval = dto.RequiresApproval;
		room.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
		room.IsActive = dto.IsActive;

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not update Room. A room with the same Code may already exist in this building.");
		}

		return NoContent();
	}

	// Delete room (hard delete for now)
	[HttpDelete("api/v1/admin/rooms/{id:int}")]
	public async Task<IActionResult> Delete(int id)
	{
		var room = await _db.Rooms.FindAsync(id);
		if (room is null) return NotFound();

		_db.Rooms.Remove(room);
		await _db.SaveChangesAsync();

		return NoContent();
	}
}
