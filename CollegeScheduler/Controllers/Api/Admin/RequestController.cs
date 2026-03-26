using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Requests;
using CollegeScheduler.Data.Entities.Facilities;
using CollegeScheduler.Data.Entities.Scheduling;
using CollegeScheduler.Data.Identity;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
public sealed class RequestController : ControllerBase
{
	private readonly ApplicationDbContext _db;

	public RequestController(ApplicationDbContext db)
	{
		_db = db;
	}

	[HttpGet("api/v1/admin/requests")]
	public async Task<ActionResult<PagedResult<RequestDto>>> GetAll([FromQuery] RequestQuery q)
	{
		var query = _db.Set<Request>()
			.AsNoTracking()
			.Include(x => x.RoomBookingDetail)
			.Include(x => x.ScheduleChangeDetail)
			.AsQueryable();

		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			var s = q.Search.Trim();
			query = query.Where(x =>
				(x.Title != null && x.Title.Contains(s)) ||
				(x.Notes != null && x.Notes.Contains(s)));
		}

		if (q.RequestTypeId.HasValue)
			query = query.Where(x => x.RequestTypeId == q.RequestTypeId.Value);

		if (q.RequestStatusId.HasValue)
			query = query.Where(x => x.RequestStatusId == q.RequestStatusId.Value);

		if (!string.IsNullOrWhiteSpace(q.RequestedByUserId))
			query = query.Where(x => x.RequestedByUserId == q.RequestedByUserId);

		if (q.IsActive.HasValue)
			query = query.Where(x => x.IsActive == q.IsActive.Value);

		var sortBy = (q.SortBy ?? "createdat").Trim().ToLowerInvariant();
		var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

		query = sortBy switch
		{
			"requesttypeid" => desc ? query.OrderByDescending(x => x.RequestTypeId) : query.OrderBy(x => x.RequestTypeId),
			"requeststatusid" => desc ? query.OrderByDescending(x => x.RequestStatusId) : query.OrderBy(x => x.RequestStatusId),
			_ => desc ? query.OrderByDescending(x => x.CreatedAtUtc) : query.OrderBy(x => x.CreatedAtUtc)
		};

		var page = q.Page < 1 ? 1 : q.Page;
		var pageSize = q.PageSize < 1 ? 20 : q.PageSize;
		if (pageSize > 100) pageSize = 100;

		var totalCount = await query.CountAsync();

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(x => new RequestDto
			{
				RequestId = x.RequestId,
				RequestTypeId = x.RequestTypeId,
				RequestStatusId = x.RequestStatusId,
				RequestedByUserId = x.RequestedByUserId,
				Title = x.Title,
				Notes = x.Notes,
				IsActive = x.IsActive,
				RoomBookingDetail = x.RoomBookingDetail == null ? null : new RequestRoomBookingDto
				{
					RequestId = x.RoomBookingDetail.RequestId,
					RoomId = x.RoomBookingDetail.RoomId,
					StartUtc = x.RoomBookingDetail.StartUtc,
					EndUtc = x.RoomBookingDetail.EndUtc,
					Purpose = x.RoomBookingDetail.Purpose,
					ExpectedAttendees = x.RoomBookingDetail.ExpectedAttendees
				},
				ScheduleChangeDetail = x.ScheduleChangeDetail == null ? null : new RequestScheduleChangeDto
				{
					RequestId = x.ScheduleChangeDetail.RequestId,
					TimetableEventId = x.ScheduleChangeDetail.TimetableEventId,
					ProposedRoomId = x.ScheduleChangeDetail.ProposedRoomId,
					ProposedStartUtc = x.ScheduleChangeDetail.ProposedStartUtc,
					ProposedEndUtc = x.ScheduleChangeDetail.ProposedEndUtc,
					Reason = x.ScheduleChangeDetail.Reason
				}
			})
			.ToListAsync();

		return Ok(new PagedResult<RequestDto>
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		});
	}

	[HttpGet("api/v1/admin/requests/{id:long}")]
	public async Task<ActionResult<RequestDto>> GetById(long id)
	{
		var item = await _db.Set<Request>()
			.AsNoTracking()
			.Include(x => x.RoomBookingDetail)
			.Include(x => x.ScheduleChangeDetail)
			.Where(x => x.RequestId == id)
			.Select(x => new RequestDto
			{
				RequestId = x.RequestId,
				RequestTypeId = x.RequestTypeId,
				RequestStatusId = x.RequestStatusId,
				RequestedByUserId = x.RequestedByUserId,
				Title = x.Title,
				Notes = x.Notes,
				IsActive = x.IsActive,
				RoomBookingDetail = x.RoomBookingDetail == null ? null : new RequestRoomBookingDto
				{
					RequestId = x.RoomBookingDetail.RequestId,
					RoomId = x.RoomBookingDetail.RoomId,
					StartUtc = x.RoomBookingDetail.StartUtc,
					EndUtc = x.RoomBookingDetail.EndUtc,
					Purpose = x.RoomBookingDetail.Purpose,
					ExpectedAttendees = x.RoomBookingDetail.ExpectedAttendees
				},
				ScheduleChangeDetail = x.ScheduleChangeDetail == null ? null : new RequestScheduleChangeDto
				{
					RequestId = x.ScheduleChangeDetail.RequestId,
					TimetableEventId = x.ScheduleChangeDetail.TimetableEventId,
					ProposedRoomId = x.ScheduleChangeDetail.ProposedRoomId,
					ProposedStartUtc = x.ScheduleChangeDetail.ProposedStartUtc,
					ProposedEndUtc = x.ScheduleChangeDetail.ProposedEndUtc,
					Reason = x.ScheduleChangeDetail.Reason
				}
			})
			.FirstOrDefaultAsync();

		return item is null ? NotFound() : Ok(item);
	}

	[HttpPost("api/v1/admin/requests")]
	public async Task<ActionResult<RequestDto>> Create([FromBody] RequestCreateDto dto)
	{
		var requestTypeExists = await _db.Set<RequestType>().AnyAsync(x => x.RequestTypeId == dto.RequestTypeId);
		if (!requestTypeExists) return NotFound($"RequestType {dto.RequestTypeId} not found.");

		var requestStatusExists = await _db.Set<RequestStatus>().AnyAsync(x => x.RequestStatusId == dto.RequestStatusId);
		if (!requestStatusExists) return NotFound($"RequestStatus {dto.RequestStatusId} not found.");

		var userExists = await _db.Users.AnyAsync(x => x.Id == dto.RequestedByUserId);
		if (!userExists) return NotFound($"User {dto.RequestedByUserId} not found.");

		var entity = new Request
		{
			RequestTypeId = dto.RequestTypeId,
			RequestStatusId = dto.RequestStatusId,
			RequestedByUserId = dto.RequestedByUserId,
			Title = dto.Title,
			Notes = dto.Notes,
			IsActive = true
		};

		if (dto.RoomBookingDetail is not null)
		{
			var roomExists = await _db.Set<Room>().AnyAsync(x => x.RoomId == dto.RoomBookingDetail.RoomId);
			if (!roomExists) return NotFound($"Room {dto.RoomBookingDetail.RoomId} not found.");

			entity.RoomBookingDetail = new RequestRoomBooking
			{
				RoomId = dto.RoomBookingDetail.RoomId,
				StartUtc = dto.RoomBookingDetail.StartUtc,
				EndUtc = dto.RoomBookingDetail.EndUtc,
				Purpose = dto.RoomBookingDetail.Purpose,
				ExpectedAttendees = dto.RoomBookingDetail.ExpectedAttendees,
				IsActive = true
			};
		}

		if (dto.ScheduleChangeDetail is not null)
		{
			var eventExists = await _db.Set<TimetableEvent>().AnyAsync(x => x.TimetableEventId == dto.ScheduleChangeDetail.TimetableEventId);
			if (!eventExists) return NotFound($"TimetableEvent {dto.ScheduleChangeDetail.TimetableEventId} not found.");

			if (dto.ScheduleChangeDetail.ProposedRoomId.HasValue)
			{
				var proposedRoomExists = await _db.Set<Room>().AnyAsync(x => x.RoomId == dto.ScheduleChangeDetail.ProposedRoomId.Value);
				if (!proposedRoomExists) return NotFound($"Proposed Room {dto.ScheduleChangeDetail.ProposedRoomId.Value} not found.");
			}

			entity.ScheduleChangeDetail = new RequestScheduleChange
			{
				TimetableEventId = dto.ScheduleChangeDetail.TimetableEventId,
				ProposedRoomId = dto.ScheduleChangeDetail.ProposedRoomId,
				ProposedStartUtc = dto.ScheduleChangeDetail.ProposedStartUtc,
				ProposedEndUtc = dto.ScheduleChangeDetail.ProposedEndUtc,
				Reason = dto.ScheduleChangeDetail.Reason,
				IsActive = true
			};
		}

		_db.Add(entity);
		await _db.SaveChangesAsync();

		return CreatedAtAction(nameof(GetById), new { id = entity.RequestId }, new RequestDto
		{
			RequestId = entity.RequestId,
			RequestTypeId = entity.RequestTypeId,
			RequestStatusId = entity.RequestStatusId,
			RequestedByUserId = entity.RequestedByUserId,
			Title = entity.Title,
			Notes = entity.Notes,
			IsActive = entity.IsActive
		});
	}

	[HttpPut("api/v1/admin/requests/{id:long}")]
	public async Task<IActionResult> Update(long id, [FromBody] RequestUpdateDto dto)
	{
		var entity = await _db.Set<Request>().FirstOrDefaultAsync(x => x.RequestId == id);
		if (entity is null) return NotFound();

		var requestTypeExists = await _db.Set<RequestType>().AnyAsync(x => x.RequestTypeId == dto.RequestTypeId);
		if (!requestTypeExists) return NotFound($"RequestType {dto.RequestTypeId} not found.");

		var requestStatusExists = await _db.Set<RequestStatus>().AnyAsync(x => x.RequestStatusId == dto.RequestStatusId);
		if (!requestStatusExists) return NotFound($"RequestStatus {dto.RequestStatusId} not found.");

		entity.RequestTypeId = dto.RequestTypeId;
		entity.RequestStatusId = dto.RequestStatusId;
		entity.Title = dto.Title;
		entity.Notes = dto.Notes;
		entity.IsActive = dto.IsActive;

		await _db.SaveChangesAsync();
		return NoContent();
	}
}