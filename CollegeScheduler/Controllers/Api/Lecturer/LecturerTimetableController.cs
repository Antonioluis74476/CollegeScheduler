using CollegeScheduler.Data;
using CollegeScheduler.Data.Identity;
using CollegeScheduler.DTOs.Requests;
using CollegeScheduler.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CollegeScheduler.Controllers.Api.Lecturer;

[ApiController]
[Route("api/v1/lecturer")]
[Authorize(Roles = RoleNames.Lecturer)]
public sealed class LecturerTimetableController : ControllerBase
{
	private readonly ApplicationDbContext _db;
	private readonly IRequestService _requestService;

	public LecturerTimetableController(
		ApplicationDbContext db,
		IRequestService requestService)
	{
		_db = db;
		_requestService = requestService;
	}

	private string CurrentUserId =>
		User.FindFirstValue(ClaimTypes.NameIdentifier)
		?? throw new UnauthorizedAccessException("Missing user id claim.");

	[HttpGet("timetable")]
	public async Task<IActionResult> GetMyTimetable(
		[FromQuery] DateTime? fromUtc,
		[FromQuery] DateTime? toUtc)
	{
		var lecturerId = await _db.LecturerProfiles
			.Where(lp => lp.UserId == CurrentUserId)
			.Select(lp => lp.LecturerId)
			.FirstOrDefaultAsync();

		if (lecturerId == 0)
			return NotFound("No lecturer profile found for the current user.");

		var query = _db.EventLecturers
			.AsNoTracking()
			.Where(el => el.LecturerId == lecturerId);

		if (fromUtc.HasValue)
			query = query.Where(el => el.TimetableEvent.EndUtc >= fromUtc.Value);

		if (toUtc.HasValue)
			query = query.Where(el => el.TimetableEvent.StartUtc <= toUtc.Value);

		var items = await query
			.OrderBy(el => el.TimetableEvent.StartUtc)
			.Select(el => new
			{
				el.TimetableEvent.TimetableEventId,
				el.TimetableEvent.StartUtc,
				el.TimetableEvent.EndUtc,
				el.TimetableEvent.SessionType,
				ModuleId = el.TimetableEvent.ModuleId,
				RoomId = el.TimetableEvent.RoomId,
				StatusId = el.TimetableEvent.EventStatusId,
				el.TimetableEvent.Notes
			})
			.ToListAsync();

		return Ok(items);
	}

	[HttpPost("requests/schedule-change")]
	public async Task<IActionResult> CreateScheduleChangeRequest(
		[FromBody] ScheduleChangeRequestCreateDto dto)
	{
		var lecturerId = await _db.LecturerProfiles
			.Where(lp => lp.UserId == CurrentUserId)
			.Select(lp => lp.LecturerId)
			.FirstOrDefaultAsync();

		if (lecturerId == 0)
			return NotFound("No lecturer profile found for the current user.");

		var isAssigned = await _db.EventLecturers
			.AnyAsync(el =>
				el.LecturerId == lecturerId &&
				el.TimetableEventId == dto.TimetableEventId);

		if (!isAssigned)
			return Forbid();

		try
		{
			var requestId = await _requestService.CreateScheduleChangeRequestAsync(
				CurrentUserId,
				dto.TimetableEventId,
				dto.ProposedRoomId,
				dto.ProposedStartUtc,
				dto.ProposedEndUtc,
				dto.Reason);

			return Ok(new
			{
				requestId,
				message = "Schedule change request submitted successfully."
			});
		}
		catch (ArgumentException ex)
		{
			return BadRequest(ex.Message);
		}
		catch (InvalidOperationException ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpGet("requests")]
	public async Task<IActionResult> GetMyRequests()
	{
		var requests = await _db.Requests
			.AsNoTracking()
			.Where(r => r.RequestedByUserId == CurrentUserId)
			.OrderByDescending(r => r.CreatedAtUtc)
			.Select(r => new
			{
				r.RequestId,
				r.Title,
				r.Notes,
				RequestType = r.RequestType.Name,
				RequestStatus = r.RequestStatus.Name,
				r.CreatedAtUtc,
				r.UpdatedAtUtc
			})
			.ToListAsync();

		return Ok(requests);
	}
}