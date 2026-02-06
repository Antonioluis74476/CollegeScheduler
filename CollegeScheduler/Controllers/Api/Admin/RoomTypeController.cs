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
public sealed class RoomTypeController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public RoomTypeController(ApplicationDbContext db)
	{
		_db = db;
	}

	[HttpGet("api/v1/admin/room-types")]
	public async Task<ActionResult<PagedResult<RoomTypeDto>>> GetAll([FromQuery] RoomTypeQuery q)
	{
		var query = _db.RoomTypes.AsNoTracking();

		// Filtering
		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			var s = q.Search.Trim();
			query = query.Where(rt => rt.Name.Contains(s));
		}

		// Sorting
		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
		query = desc ? query.OrderByDescending(rt => rt.Name) : query.OrderBy(rt => rt.Name);

		// Paging
		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(rt => new RoomTypeDto
			{
				RoomTypeId = rt.RoomTypeId,
				Name = rt.Name
			})
			.ToListAsync();

		return Ok(new PagedResult<RoomTypeDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	[HttpGet("api/v1/admin/room-types/{id:int}")]
	public async Task<ActionResult<RoomTypeDto>> GetById(int id)
	{
		var item = await _db.RoomTypes
			.AsNoTracking()
			.Where(rt => rt.RoomTypeId == id)
			.Select(rt => new RoomTypeDto
			{
				RoomTypeId = rt.RoomTypeId,
				Name = rt.Name
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	[HttpPost("api/v1/admin/room-types")]
	public async Task<ActionResult<RoomTypeDto>> Create([FromBody] RoomTypeCreateDto dto)
	{
		var roomType = new RoomType
		{
			Name = dto.Name.Trim()
		};

		_db.RoomTypes.Add(roomType);

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not save RoomType. A RoomType with the same Name may already exist.");
		}

		var result = new RoomTypeDto
		{
			RoomTypeId = roomType.RoomTypeId,
			Name = roomType.Name
		};

		return CreatedAtAction(nameof(GetById), new { id = roomType.RoomTypeId }, result);
	}

	[HttpPut("api/v1/admin/room-types/{id:int}")]
	public async Task<IActionResult> Update(int id, [FromBody] RoomTypeUpdateDto dto)
	{
		var roomType = await _db.RoomTypes.FirstOrDefaultAsync(rt => rt.RoomTypeId == id);
		if (roomType is null) return NotFound();

		roomType.Name = dto.Name.Trim();

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict("Could not update RoomType. A RoomType with the same Name may already exist.");
		}

		return NoContent();
	}

	[HttpDelete("api/v1/admin/room-types/{id:int}")]
	public async Task<IActionResult> Delete(int id)
	{
		var roomType = await _db.RoomTypes.FindAsync(id);
		if (roomType is null) return NotFound();

		// Safety: prevent deleting if referenced by any Room
		var inUse = await _db.Rooms.AsNoTracking().AnyAsync(r => r.RoomTypeId == id);
		if (inUse)
			return Conflict("Cannot delete this RoomType because Rooms are using it. Deactivate it or migrate rooms first.");

		_db.RoomTypes.Remove(roomType);
		await _db.SaveChangesAsync();

		return NoContent();
	}
}
