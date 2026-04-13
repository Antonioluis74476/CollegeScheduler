using CollegeScheduler.Data;
using CollegeScheduler.Data.Identity;
using CollegeScheduler.DTOs.Requests;
using CollegeScheduler.DTOs.Profiles;
using CollegeScheduler.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
	private readonly UserManager<ApplicationUser> _userManager;

	public LecturerTimetableController(
		ApplicationDbContext db,
		IRequestService requestService,
		UserManager<ApplicationUser> userManager)
	{
		_db = db;
		_requestService = requestService;
		_userManager = userManager;
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

	[HttpPost("requests/cancel-class")]
	public async Task<IActionResult> CreateCancelClassRequest(
		[FromBody] CancelClassRequestCreateDto dto)
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
			var requestId = await _requestService.CreateCancelClassRequestAsync(
				CurrentUserId,
				dto.TimetableEventId,
				dto.Reason);

			return Ok(new
			{
				requestId,
				message = "Cancel class request submitted successfully."
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

	[HttpPost("requests/room-booking")]
	public async Task<IActionResult> CreateRoomBookingRequest(
		[FromBody] LecturerRoomBookingRequestCreateDto dto)
	{
		try
		{
			var requestId = await _requestService.CreateRoomBookingRequestAsync(
				CurrentUserId,
				dto.RoomId,
				dto.StartUtc,
				dto.EndUtc,
				dto.Purpose,
				dto.ExpectedAttendees);

			return Ok(new
			{
				requestId,
				message = "Room booking request submitted successfully."
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

	[HttpGet("notifications")]
	public async Task<IActionResult> GetMyNotifications([FromQuery] bool unreadOnly = false)
	{
		var query = _db.NotificationRecipients
			.AsNoTracking()
			.Where(nr => nr.UserId == CurrentUserId);

		if (unreadOnly)
			query = query.Where(nr => nr.ReadAtUtc == null);

		var notifications = await query
			.OrderByDescending(nr => nr.Notification.CreatedAtUtc)
			.Select(nr => new
			{
				nr.Notification.NotificationId,
				nr.Notification.Title,
				nr.Notification.Message,
				nr.Notification.CreatedAtUtc,
				nr.DeliveryStatus,
				nr.ReadAtUtc,
				nr.Notification.RelatedTimetableEventId,
				nr.Notification.RelatedRequestId
			})
			.ToListAsync();

		return Ok(notifications);
	}

	[HttpGet("profile")]
	public async Task<IActionResult> GetMyProfile()
	{
		var profile = await _db.LecturerProfiles
			.AsNoTracking()
			.Where(lp => lp.UserId == CurrentUserId)
			.Select(lp => new
			{
				lp.LecturerId,
				lp.StaffNumber,
				lp.Name,
				lp.LastName,
				lp.Email,
				lp.DepartmentId
			})
			.FirstOrDefaultAsync();

		if (profile is null)
			return NotFound("No lecturer profile found for the current user.");

		return Ok(profile);
	}

	[HttpPost("profile/change-password")]
	public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
	{
		if (string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
			string.IsNullOrWhiteSpace(dto.NewPassword))
		{
			return BadRequest("CurrentPassword and NewPassword are required.");
		}

		var user = await _userManager.FindByIdAsync(CurrentUserId);
		if (user is null)
			return NotFound("User not found.");

		var result = await _userManager.ChangePasswordAsync(
			user,
			dto.CurrentPassword,
			dto.NewPassword);

		if (!result.Succeeded)
		{
			return BadRequest(new
			{
				message = "Password change failed.",
				errors = result.Errors.Select(e => e.Description).ToList()
			});
		}

		return Ok(new
		{
			message = "Password changed successfully."
		});
	}
}