using CollegeScheduler.Data;
using CollegeScheduler.Data.Identity;
using CollegeScheduler.DTOs.Profiles;
using CollegeScheduler.DTOs.Requests;
using CollegeScheduler.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CollegeScheduler.Controllers.Api.Student;

[ApiController]
[Route("api/v1/student")]
[Authorize(Roles = RoleNames.Student)]
public sealed class StudentTimetableController : ControllerBase
{
	private readonly ApplicationDbContext _db;
	private readonly IRequestService _requestService;
	private readonly UserManager<ApplicationUser> _userManager;

	public StudentTimetableController(
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
		var studentId = await _db.StudentProfiles
			.Where(sp => sp.UserId == CurrentUserId)
			.Select(sp => sp.StudentId)
			.FirstOrDefaultAsync();

		if (studentId == 0)
			return NotFound("No student profile found for the current user.");

		var cohortIds = await _db.StudentCohortMemberships
			.AsNoTracking()
			.Where(x => x.StudentId == studentId)
			.Select(x => x.CohortId)
			.Distinct()
			.ToListAsync();

		if (cohortIds.Count == 0)
			return Ok(new List<object>());

		var query = _db.EventCohorts
			.AsNoTracking()
			.Where(ec => cohortIds.Contains(ec.CohortId));

		if (fromUtc.HasValue)
			query = query.Where(ec => ec.TimetableEvent.EndUtc >= fromUtc.Value);

		if (toUtc.HasValue)
			query = query.Where(ec => ec.TimetableEvent.StartUtc <= toUtc.Value);

		var items = await query
			.OrderBy(ec => ec.TimetableEvent.StartUtc)
			.Select(ec => new
			{
				ec.TimetableEvent.TimetableEventId,
				ec.CohortId,
				ec.TimetableEvent.StartUtc,
				ec.TimetableEvent.EndUtc,
				ec.TimetableEvent.SessionType,
				ModuleId = ec.TimetableEvent.ModuleId,
				RoomId = ec.TimetableEvent.RoomId,
				StatusId = ec.TimetableEvent.EventStatusId,
				ec.TimetableEvent.Notes
			})
			.Distinct()
			.ToListAsync();

		return Ok(items);
	}

	[HttpGet("notifications")]
	public async Task<IActionResult> GetMyNotifications([FromQuery] bool unreadOnly = false)
	{
		var query = _db.NotificationRecipients
			.AsNoTracking()
			.Where(nr => nr.UserId == CurrentUserId);

		if (unreadOnly)
			query = query.Where(nr => nr.ReadAtUtc == null);

		var items = await query
			.OrderByDescending(nr => nr.Notification.CreatedAtUtc)
			.Select(nr => new
			{
				nr.NotificationId,
				nr.Notification.Title,
				nr.Notification.Message,
				nr.Notification.CreatedAtUtc,
				nr.DeliveryStatus,
				nr.ReadAtUtc,
				nr.Notification.RelatedTimetableEventId,
				nr.Notification.RelatedRequestId
			})
			.ToListAsync();

		return Ok(items);
	}

	[HttpPost("notifications/{id:long}/read")]
	public async Task<IActionResult> MarkNotificationAsRead(long id)
	{
		var row = await _db.NotificationRecipients
			.FirstOrDefaultAsync(nr =>
				nr.NotificationId == id &&
				nr.UserId == CurrentUserId);

		if (row is null)
			return NotFound();

		row.ReadAtUtc = DateTime.UtcNow;
		await _db.SaveChangesAsync();

		return NoContent();
	}

	[HttpPost("requests/room-booking")]
	public async Task<IActionResult> CreateRoomBookingRequest(
		[FromBody] RoomBookingRequestCreateDto dto)
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

	[HttpGet("profile")]
	public async Task<IActionResult> GetMyProfile()
	{
		var profile = await _db.StudentProfiles
			.AsNoTracking()
			.Where(sp => sp.UserId == CurrentUserId)
			.Select(sp => new
			{
				sp.StudentId,
				sp.StudentNumber,
				sp.Name,
				sp.LastName,
				sp.Email,
				sp.Status
			})
			.FirstOrDefaultAsync();

		if (profile is null)
			return NotFound("No student profile found for the current user.");

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