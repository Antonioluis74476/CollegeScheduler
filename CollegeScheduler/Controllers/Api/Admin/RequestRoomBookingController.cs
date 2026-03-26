using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Facilities;
using CollegeScheduler.Data.Entities.Requests;
using CollegeScheduler.Data.Identity;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
public sealed class RequestRoomBookingController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public RequestRoomBookingController(ApplicationDbContext db)
	{
		_db = db;
	}

	// LIST
	[HttpGet("api/v1/admin/request-room-bookings")]
	public async Task<ActionResult<PagedResult<RequestRoomBookingDto>>> GetAll([FromQuery] RequestRoomBookingQuery q)
	{
		var query = _db.Set<RequestRoomBooking>()
			.AsNoTracking()
			.AsQueryable();

		if (q.RoomId.HasValue)
			query = query.Where(x => x.RoomId == q.RoomId.Value);

		if (q.RequestId.HasValue)
			query = query.Where(x => x.RequestId == q.RequestId.Value);

		if (q.IsActive.HasValue)
			query = query.Where(x => x.IsActive == q.IsActive.Value);

		var sortBy = (q.SortBy ?? "startutc").Trim().ToLowerInvariant();
		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

		query = sortBy switch
		{
			"roomid" => desc ? query.OrderByDescending(x => x.RoomId) : query.OrderBy(x => x.RoomId),
			"endutc" => desc ? query.OrderByDescending(x => x.EndUtc) : query.OrderBy(x => x.EndUtc),
			"createdat" => desc ? query.OrderByDescending(x => x.CreatedAtUtc) : query.OrderBy(x => x.CreatedAtUtc),
			_ => desc ? query.OrderByDescending(x => x.StartUtc) : query.OrderBy(x => x.StartUtc),
		};

		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new RequestRoomBookingDto
			{
				RequestId = x.RequestId,
				RoomId = x.RoomId,
				StartUtc = x.StartUtc,
				EndUtc = x.EndUtc,
				Purpose = x.Purpose,
				ExpectedAttendees = x.ExpectedAttendees
			})
			.ToListAsync();

		return Ok(new PagedResult<RequestRoomBookingDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	// GET by request id
	[HttpGet("api/v1/admin/request-room-bookings/{requestId:long}")]
	public async Task<ActionResult<RequestRoomBookingDto>> GetById(long requestId)
	{
		var item = await _db.Set<RequestRoomBooking>()
			.AsNoTracking()
			.Where(x => x.RequestId == requestId)
			.Select(x => new RequestRoomBookingDto
			{
				RequestId = x.RequestId,
				RoomId = x.RoomId,
				StartUtc = x.StartUtc,
				EndUtc = x.EndUtc,
				Purpose = x.Purpose,
				ExpectedAttendees = x.ExpectedAttendees
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	// CREATE
	[HttpPost("api/v1/admin/request-room-bookings")]
	public async Task<ActionResult<RequestRoomBookingDto>> Create([FromBody] RequestRoomBookingCreateDto dto, [FromQuery] long requestId)
	{
		var request = await _db.Set<Request>().FirstOrDefaultAsync(x => x.RequestId == requestId);
		if (request is null) return NotFound($"Request {requestId} not found.");

		var roomExists = await _db.Set<Room>().AnyAsync(x => x.RoomId == dto.RoomId);
		if (!roomExists) return NotFound($"Room {dto.RoomId} not found.");

		var alreadyExists = await _db.Set<RequestRoomBooking>().AnyAsync(x => x.RequestId == requestId);
		if (alreadyExists) return Conflict($"RequestRoomBooking for Request {requestId} already exists.");

		var entity = new RequestRoomBooking
		{
			RequestId = requestId,
			RoomId = dto.RoomId,
			StartUtc = dto.StartUtc,
			EndUtc = dto.EndUtc,
			Purpose = dto.Purpose,
			ExpectedAttendees = dto.ExpectedAttendees,
			IsActive = true
		};

		_db.Add(entity);
		await _db.SaveChangesAsync();

		return CreatedAtAction(nameof(GetById), new { requestId = entity.RequestId }, new RequestRoomBookingDto
		{
			RequestId = entity.RequestId,
			RoomId = entity.RoomId,
			StartUtc = entity.StartUtc,
			EndUtc = entity.EndUtc,
			Purpose = entity.Purpose,
			ExpectedAttendees = entity.ExpectedAttendees
		});
	}

	// UPDATE
	[HttpPut("api/v1/admin/request-room-bookings/{requestId:long}")]
	public async Task<IActionResult> Update(long requestId, [FromBody] RequestRoomBookingUpdateDto dto)
	{
		var entity = await _db.Set<RequestRoomBooking>().FirstOrDefaultAsync(x => x.RequestId == requestId);
		if (entity is null) return NotFound();

		var roomExists = await _db.Set<Room>().AnyAsync(x => x.RoomId == dto.RoomId);
		if (!roomExists) return NotFound($"Room {dto.RoomId} not found.");

		entity.RoomId = dto.RoomId;
		entity.StartUtc = dto.StartUtc;
		entity.EndUtc = dto.EndUtc;
		entity.Purpose = dto.Purpose;
		entity.ExpectedAttendees = dto.ExpectedAttendees;
		entity.IsActive = dto.IsActive;

		await _db.SaveChangesAsync();
		return NoContent();
	}
}