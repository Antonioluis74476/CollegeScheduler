using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Facilities;
using CollegeScheduler.DTOs.Facilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
public sealed class RoomFeaturesController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public RoomFeaturesController(ApplicationDbContext db)
	{
		_db = db;
	}

	// List features assigned to a room
	[HttpGet("api/v1/admin/rooms/{roomId:int}/features")]
	public async Task<ActionResult<List<RoomFeatureDto>>> GetForRoom(int roomId)
	{
		var roomExists = await _db.Rooms.AsNoTracking().AnyAsync(r => r.RoomId == roomId);
		if (!roomExists) return NotFound($"Room {roomId} not found.");

		var items = await _db.RoomFeatures
			.AsNoTracking()
			.Where(rf => rf.RoomId == roomId)
			.Include(rf => rf.Feature)
			.OrderBy(rf => rf.Feature!.Name)
			.Select(rf => new RoomFeatureDto
			{
				FeatureId = rf.FeatureId,
				Name = rf.Feature!.Name
			})
			.ToListAsync();

		return Ok(items);
	}

	// Assign a feature to a room
	[HttpPost("api/v1/admin/rooms/{roomId:int}/features")]
	public async Task<IActionResult> AddToRoom(int roomId, [FromBody] RoomFeatureAddDto dto)
	{
		var roomExists = await _db.Rooms.AnyAsync(r => r.RoomId == roomId);
		if (!roomExists) return NotFound($"Room {roomId} not found.");

		var featureExists = await _db.Features.AnyAsync(f => f.FeatureId == dto.FeatureId);
		if (!featureExists) return NotFound($"Feature {dto.FeatureId} not found.");

		var alreadyExists = await _db.RoomFeatures.AnyAsync(rf =>
			rf.RoomId == roomId && rf.FeatureId == dto.FeatureId);

		if (alreadyExists) return Conflict("This feature is already assigned to the room.");

		_db.RoomFeatures.Add(new RoomFeature
		{
			RoomId = roomId,
			FeatureId = dto.FeatureId
		});

		await _db.SaveChangesAsync();
		return NoContent();
	}

	// Remove a feature from a room (composite key)
	[HttpDelete("api/v1/admin/rooms/{roomId:int}/features/{featureId:int}")]
	public async Task<IActionResult> RemoveFromRoom(int roomId, int featureId)
	{
		var row = await _db.RoomFeatures
			.FirstOrDefaultAsync(rf => rf.RoomId == roomId && rf.FeatureId == featureId);

		if (row is null) return NotFound();

		_db.RoomFeatures.Remove(row);
		await _db.SaveChangesAsync();

		return NoContent();
	}
}
