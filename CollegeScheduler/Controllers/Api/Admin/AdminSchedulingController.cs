using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Scheduling;
using CollegeScheduler.Data.Identity;
using CollegeScheduler.DTOs.Requests;
using CollegeScheduler.DTOs.Scheduling;
using CollegeScheduler.Hubs;
using CollegeScheduler.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CollegeScheduler.Controllers.Api.Admin;

[ApiController]
[Route("api/v1/admin/scheduling")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class AdminSchedulingController : ControllerBase
{
	private readonly ApplicationDbContext _db;
	private readonly ISchedulingService _schedulingService;
	private readonly IRequestService _requestService;
	private readonly INotificationService _notificationService;
	private readonly TimetableHubNotifier _hubNotifier;

	public AdminSchedulingController(
		ApplicationDbContext db,
		ISchedulingService schedulingService,
		IRequestService requestService,
		INotificationService notificationService,
		TimetableHubNotifier hubNotifier)
	{
		_db = db;
		_schedulingService = schedulingService;
		_requestService = requestService;
		_notificationService = notificationService;
		_hubNotifier = hubNotifier;
	}

	private string CurrentUserId =>
		User.FindFirstValue(ClaimTypes.NameIdentifier)
		?? throw new UnauthorizedAccessException("Missing user id claim.");

	[HttpGet("rooms/available")]
	public async Task<IActionResult> FindAvailableRooms(
		[FromQuery] DateTime startUtc,
		[FromQuery] DateTime endUtc,
		[FromQuery] int? minCapacity,
		[FromQuery] int? roomTypeId,
		[FromQuery] int? buildingId,
		[FromQuery] int? campusId,
		[FromQuery] List<int>? featureIds)
	{
		try
		{
			var rooms = await _schedulingService.FindAvailableRoomsAsync(new RoomSearchQuery
			{
				StartUtc = startUtc,
				EndUtc = endUtc,
				MinCapacity = minCapacity,
				RoomTypeId = roomTypeId,
				BuildingId = buildingId,
				CampusId = campusId,
				RequiredFeatureIds = featureIds
			});

			return Ok(rooms);
		}
		catch (ArgumentException ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpPost("check-clashes")]
	public async Task<IActionResult> CheckClashes([FromBody] ClashCheckRequest dto)
	{
		try
		{
			var result = await _schedulingService.CheckClashesAsync(
				dto.ExcludeEventId,
				dto.RoomId,
				dto.StartUtc,
				dto.EndUtc,
				dto.CohortIds,
				dto.LecturerIds);

			if (!result.HasClash)
			{
				return Ok(new
				{
					hasClash = false,
					message = "No clashes detected."
				});
			}

			return Conflict(new
			{
				hasClash = true,
				roomClash = result.RoomClash,
				cohortClashes = result.CohortClashes,
				lecturerClashes = result.LecturerClashes
			});
		}
		catch (ArgumentException ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpPost("recurring-events")]
	public async Task<IActionResult> CreateRecurringEvents([FromBody] RecurringEventCreateDto dto)
	{
		try
		{
			var events = await _schedulingService.GenerateRecurringEventsAsync(dto, CurrentUserId);

			if (events.Count == 0)
			{
				return BadRequest("No recurring events could be created. All weeks may have clashes or term dates may not match.");
			}

			_db.TimetableEvents.AddRange(events);
			await _db.SaveChangesAsync();

			foreach (var ev in events)
			{
				foreach (var cohortId in dto.CohortIds.Distinct())
				{
					_db.EventCohorts.Add(new EventCohort
					{
						TimetableEventId = ev.TimetableEventId,
						CohortId = cohortId
					});
				}

				foreach (var lecturerId in dto.LecturerIds.Distinct())
				{
					_db.EventLecturers.Add(new EventLecturer
					{
						TimetableEventId = ev.TimetableEventId,
						LecturerId = lecturerId
					});
				}
			}

			await _db.SaveChangesAsync();

			return Ok(new
			{
				createdCount = events.Count,
				recurrenceGroupId = events.First().RecurrenceGroupId,
				eventIds = events.Select(e => e.TimetableEventId).ToList()
			});
		}
		catch (ArgumentException ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpGet("requests/pending")]
	public async Task<IActionResult> GetPendingRequests()
	{
		var pendingRequests = await _db.Requests
			.AsNoTracking()
			.Where(r => r.RequestStatus.Name == "Pending")
			.OrderBy(r => r.CreatedAtUtc)
			.Select(r => new
			{
				r.RequestId,
				r.Title,
				r.Notes,
				RequestType = r.RequestType.Name,
				RequestStatus = r.RequestStatus.Name,
				r.RequestedByUserId,
				r.CreatedAtUtc,

				ScheduleChangeDetail = _db.RequestScheduleChanges
					.Where(sc => sc.RequestId == r.RequestId)
					.Select(sc => new
					{
						sc.TimetableEventId,
						sc.ProposedRoomId,
						sc.ProposedStartUtc,
						sc.ProposedEndUtc,
						sc.Reason
					})
					.FirstOrDefault(),

				RoomBookingDetail = _db.RequestRoomBookings
					.Where(rb => rb.RequestId == r.RequestId)
					.Select(rb => new
					{
						rb.RoomId,
						rb.StartUtc,
						rb.EndUtc,
						rb.Purpose,
						rb.ExpectedAttendees
					})
					.FirstOrDefault()
			})
			.ToListAsync();

		return Ok(pendingRequests);
	}

	[HttpPost("requests/{id:long}/decide")]
	public async Task<IActionResult> DecideRequest(long id, [FromBody] DecideRequestDto dto)
	{
		try
		{
			var result = await _requestService.DecideAsync(
				requestId: id,
				decidedByUserId: CurrentUserId,
				decision: dto.Decision,
				comment: dto.Comment);

			return result.IsSuccess ? Ok(result) : Conflict(result);
		}
		catch (ArgumentException ex)
		{
			return BadRequest(ex.Message);
		}
		catch (InvalidOperationException ex)
		{
			return Conflict(ex.Message);
		}
	}

	[HttpPost("events/{id:long}/reschedule")]
	public async Task<IActionResult> RescheduleEvent(long id, [FromBody] AdminEventRescheduleDto dto)
	{
		if (dto.EndUtc <= dto.StartUtc)
			return BadRequest("EndUtc must be greater than StartUtc.");

		var timetableEvent = await _db.TimetableEvents
			.Include(te => te.EventCohorts)
			.Include(te => te.EventLecturers)
			.FirstOrDefaultAsync(te => te.TimetableEventId == id);

		if (timetableEvent is null)
			return NotFound($"TimetableEvent {id} not found.");

		var cohortIds = timetableEvent.EventCohorts.Select(ec => ec.CohortId).ToList();
		var lecturerIds = timetableEvent.EventLecturers.Select(el => el.LecturerId).ToList();

		var clashResult = await _schedulingService.CheckClashesAsync(
			excludeEventId: timetableEvent.TimetableEventId,
			roomId: dto.RoomId,
			startUtc: dto.StartUtc,
			endUtc: dto.EndUtc,
			cohortIds: cohortIds,
			lecturerIds: lecturerIds);

		if (clashResult.HasClash)
			return Conflict(new
			{
				message = "Reschedule cannot be applied because it creates a clash.",
				roomClash = clashResult.RoomClash,
				cohortClashes = clashResult.CohortClashes,
				lecturerClashes = clashResult.LecturerClashes
			});

		var oldRoomId = timetableEvent.RoomId;
		var oldStartUtc = timetableEvent.StartUtc;
		var oldEndUtc = timetableEvent.EndUtc;

		timetableEvent.RoomId = dto.RoomId;
		timetableEvent.StartUtc = dto.StartUtc;
		timetableEvent.EndUtc = dto.EndUtc;

		_db.TimetableEventChanges.Add(new TimetableEventChange
		{
			TimetableEventId = timetableEvent.TimetableEventId,
			ChangeType = "AdminReschedule",
			OldRoomId = oldRoomId,
			NewRoomId = dto.RoomId,
			OldStartUtc = oldStartUtc,
			NewStartUtc = dto.StartUtc,
			OldEndUtc = oldEndUtc,
			NewEndUtc = dto.EndUtc,
			Reason = string.IsNullOrWhiteSpace(dto.Reason) ? "Rescheduled by admin." : dto.Reason,
			ChangedByUserId = CurrentUserId,
			ChangedAtUtc = DateTime.UtcNow,
			NotificationSent = false
		});

		await _db.SaveChangesAsync();

		var recipientUserIds = new List<string>();

		var studentUserIds = await (
			from scm in _db.StudentCohortMemberships
			join sp in _db.StudentProfiles on scm.StudentId equals sp.StudentId
			where cohortIds.Contains(scm.CohortId) && sp.UserId != null
			select sp.UserId!
		)
		.Distinct()
		.ToListAsync();

		recipientUserIds.AddRange(studentUserIds);

		var lecturerUserIds = await _db.LecturerProfiles
			.Where(lp => lecturerIds.Contains(lp.LecturerId) && lp.UserId != null)
			.Select(lp => lp.UserId!)
			.Distinct()
			.ToListAsync();

		recipientUserIds.AddRange(lecturerUserIds);

		if (recipientUserIds.Count > 0)
		{
			await _notificationService.CreateAsync(
				notificationTypeName: "EventChanged",
				title: "Class schedule changed",
				message: $"Timetable event #{timetableEvent.TimetableEventId} has been rescheduled by admin.",
				recipientUserIds: recipientUserIds.Distinct(),
				relatedTimetableEventId: timetableEvent.TimetableEventId);
		}

		await _hubNotifier.PushEventChangedAsync(
			timetableEventId: timetableEvent.TimetableEventId,
			cohortIds: cohortIds,
			lecturerUserIds: lecturerUserIds,
			oldStartUtc: oldStartUtc,
			newStartUtc: dto.StartUtc);

		var lastChange = await _db.TimetableEventChanges
			.Where(x => x.TimetableEventId == timetableEvent.TimetableEventId)
			.OrderByDescending(x => x.ChangedAtUtc)
			.FirstOrDefaultAsync();

		if (lastChange is not null)
		{
			lastChange.NotificationSent = true;
			await _db.SaveChangesAsync();
		}

		return Ok(new
		{
			message = "Event rescheduled successfully.",
			timetableEventId = timetableEvent.TimetableEventId
		});
	}

	[HttpPost("events/{id:long}/cancel")]
	public async Task<IActionResult> CancelEvent(long id, [FromBody] AdminCancelEventDto dto)
	{
		var timetableEvent = await _db.TimetableEvents
			.Include(te => te.EventCohorts)
			.Include(te => te.EventLecturers)
			.FirstOrDefaultAsync(te => te.TimetableEventId == id);

		if (timetableEvent is null)
			return NotFound($"TimetableEvent {id} not found.");

		var cancelledStatusId = await _db.EventStatuses
			.Where(x => x.Name == "Cancelled")
			.Select(x => x.EventStatusId)
			.FirstOrDefaultAsync();

		if (cancelledStatusId == 0)
			return InvalidOperation("EventStatus 'Cancelled' not found.");

		var cohortIds = timetableEvent.EventCohorts.Select(ec => ec.CohortId).ToList();
		var lecturerIds = timetableEvent.EventLecturers.Select(el => el.LecturerId).ToList();

		timetableEvent.EventStatusId = cancelledStatusId;

		_db.TimetableEventChanges.Add(new TimetableEventChange
		{
			TimetableEventId = timetableEvent.TimetableEventId,
			ChangeType = "AdminCancellation",
			Reason = string.IsNullOrWhiteSpace(dto.Reason) ? "Cancelled by admin." : dto.Reason,
			ChangedByUserId = CurrentUserId,
			ChangedAtUtc = DateTime.UtcNow,
			NotificationSent = false
		});

		await _db.SaveChangesAsync();

		var recipientUserIds = new List<string>();

		var studentUserIds = await (
			from scm in _db.StudentCohortMemberships
			join sp in _db.StudentProfiles on scm.StudentId equals sp.StudentId
			where cohortIds.Contains(scm.CohortId) && sp.UserId != null
			select sp.UserId!
		)
		.Distinct()
		.ToListAsync();

		recipientUserIds.AddRange(studentUserIds);

		var lecturerUserIds = await _db.LecturerProfiles
			.Where(lp => lecturerIds.Contains(lp.LecturerId) && lp.UserId != null)
			.Select(lp => lp.UserId!)
			.Distinct()
			.ToListAsync();

		recipientUserIds.AddRange(lecturerUserIds);

		if (recipientUserIds.Count > 0)
		{
			await _notificationService.CreateAsync(
				notificationTypeName: "EventCancelled",
				title: "Class cancelled",
				message: $"Timetable event #{timetableEvent.TimetableEventId} has been cancelled by admin.",
				recipientUserIds: recipientUserIds.Distinct(),
				relatedTimetableEventId: timetableEvent.TimetableEventId);
		}

		await _hubNotifier.PushEventCancelledAsync(
			timetableEventId: timetableEvent.TimetableEventId,
			cohortIds: cohortIds,
			lecturerUserIds: lecturerUserIds,
			reason: dto.Reason);

		var lastChange = await _db.TimetableEventChanges
			.Where(x => x.TimetableEventId == timetableEvent.TimetableEventId)
			.OrderByDescending(x => x.ChangedAtUtc)
			.FirstOrDefaultAsync();

		if (lastChange is not null)
		{
			lastChange.NotificationSent = true;
			await _db.SaveChangesAsync();
		}

		return Ok(new
		{
			message = "Event cancelled successfully.",
			timetableEventId = timetableEvent.TimetableEventId
		});
	}

	private IActionResult InvalidOperation(string message)
	{
		return Conflict(message);
	}
}