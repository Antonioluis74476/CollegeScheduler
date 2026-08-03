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

    [HttpPost("rooms/recurring-available")]
    public async Task<IActionResult> FindRecurringAvailableRooms(
    [FromBody] RecurringRoomSearchQuery query)
    {
        try
        {
            var rooms = await _schedulingService
                .FindRecurringAvailableRoomsAsync(query);

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
	[HttpGet("recurring-events/{recurrenceGroupId:guid}")]
	public async Task<IActionResult> GetRecurringEvents(Guid recurrenceGroupId)
	{
		var events = await _schedulingService.GetRecurringEventSeriesAsync(recurrenceGroupId);

		if (events.Count == 0)
			return NotFound($"No recurring events found for group {recurrenceGroupId}.");

		return Ok(new RecurringEventSeriesDto
		{
			RecurrenceGroupId = recurrenceGroupId,
			Occurrences = events.Select(e => new TimetableEventDto
			{
				TimetableEventId = e.TimetableEventId,
				TermId = e.TermId,
				ModuleId = e.ModuleId,
				RoomId = e.RoomId,
				StartUtc = e.StartUtc,
				EndUtc = e.EndUtc,
				EventStatusId = e.EventStatusId,
				SessionType = e.SessionType,
				RecurrenceGroupId = e.RecurrenceGroupId,
				Notes = e.Notes,
				CreatedByUserId = e.CreatedByUserId,
				CreatedAtUtc = e.CreatedAtUtc,
				UpdatedAtUtc = e.UpdatedAtUtc
			}).ToList()
		});
	}

	[HttpPut("recurring-events/{recurrenceGroupId:guid}")]
	public async Task<IActionResult> UpdateRecurringEvents(Guid recurrenceGroupId, [FromBody] UpdateRecurringEventDto dto)
	{
		try
		{
			var result = await _schedulingService.UpdateRecurringEventsAsync(recurrenceGroupId, dto, CurrentUserId);

			if (!result.Success)
				return Conflict(new
				{
					message = "Update cannot be applied because it creates one or more clashes.",
					clashes = result.Clashes
				});

			return Ok(result);
		}
		catch (ArgumentException ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpPatch("recurring-events/{recurrenceGroupId:guid}/cancel")]
	public async Task<IActionResult> CancelRecurringEvents(Guid recurrenceGroupId, [FromBody] CancelRecurringEventDto dto)
	{
		try
		{
			var result = await _schedulingService.CancelRecurringEventsAsync(recurrenceGroupId, dto, CurrentUserId);
			return Ok(result);
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

                RequestedByEmail = _db.Users
					.Where(u => u.Id == r.RequestedByUserId)
					.Select(u => u.Email)
					.FirstOrDefault(),

                RequestedByName =
					_db.StudentProfiles
						.Where(sp => sp.UserId == r.RequestedByUserId)
						.Select(sp => (sp.Name + " " + sp.LastName).Trim())
						.FirstOrDefault()
					??
					_db.LecturerProfiles
						.Where(lp => lp.UserId == r.RequestedByUserId)
						.Select(lp => (lp.Name + " " + lp.LastName).Trim())
						.FirstOrDefault(),

                RequestedByRole =
					_db.StudentProfiles.Any(
						sp => sp.UserId == r.RequestedByUserId)
						? "Student"
						: _db.LecturerProfiles.Any(
							lp => lp.UserId == r.RequestedByUserId)
							? "Lecturer"
							: "User",

                r.CreatedAtUtc,


                ScheduleChangeDetail = _db.RequestScheduleChanges
					.Where(sc => sc.RequestId == r.RequestId)
					.Select(sc => new
					{
						sc.TimetableEventId,

						ModuleCode = _db.TimetableEvents
							.Where(te => te.TimetableEventId == sc.TimetableEventId)
							.Select(te => te.Module.Code)
							.FirstOrDefault(),

						ModuleTitle = _db.TimetableEvents
							.Where(te => te.TimetableEventId == sc.TimetableEventId)
							.Select(te => te.Module.Title)
							.FirstOrDefault(),

						CurrentRoomId = _db.TimetableEvents
							.Where(te => te.TimetableEventId == sc.TimetableEventId)
							.Select(te => te.RoomId)
							.FirstOrDefault(),

						CurrentRoomCode = _db.TimetableEvents
							.Where(te => te.TimetableEventId == sc.TimetableEventId)
							.Select(te => te.Room.Code)
							.FirstOrDefault(),

						CurrentRoomName = _db.TimetableEvents
							.Where(te => te.TimetableEventId == sc.TimetableEventId)
							.Select(te => te.Room.Name)
							.FirstOrDefault(),

						CurrentStartUtc = _db.TimetableEvents
							.Where(te => te.TimetableEventId == sc.TimetableEventId)
							.Select(te => te.StartUtc)
							.FirstOrDefault(),

						CurrentEndUtc = _db.TimetableEvents
							.Where(te => te.TimetableEventId == sc.TimetableEventId)
							.Select(te => te.EndUtc)
							.FirstOrDefault(),

						sc.ProposedRoomId,

						ProposedRoomCode = sc.ProposedRoomId.HasValue
							? _db.Rooms
								.Where(room => room.RoomId == sc.ProposedRoomId.Value)
								.Select(room => room.Code)
								.FirstOrDefault()
							: null,

						ProposedRoomName = sc.ProposedRoomId.HasValue
							? _db.Rooms
								.Where(room => room.RoomId == sc.ProposedRoomId.Value)
								.Select(room => room.Name)
								.FirstOrDefault()
							: null,

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

						RoomCode = _db.Rooms
							.Where(room => room.RoomId == rb.RoomId)
							.Select(room => room.Code)
							.FirstOrDefault(),

						RoomName = _db.Rooms
							.Where(room => room.RoomId == rb.RoomId)
							.Select(room => room.Name)
							.FirstOrDefault(),

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

	[HttpGet("events")]
	public async Task<IActionResult> GetEvents([FromQuery] TimetableEventListQuery query)
	{
		var eventsQuery = _db.TimetableEvents.AsNoTracking().AsQueryable();

		if (query.TermId.HasValue)
			eventsQuery = eventsQuery.Where(e => e.TermId == query.TermId.Value);

		if (query.ModuleId.HasValue)
			eventsQuery = eventsQuery.Where(e => e.ModuleId == query.ModuleId.Value);

		if (query.RoomId.HasValue)
			eventsQuery = eventsQuery.Where(e => e.RoomId == query.RoomId.Value);

		if (query.CohortId.HasValue)
			eventsQuery = eventsQuery.Where(e => e.EventCohorts.Any(ec => ec.CohortId == query.CohortId.Value));

		if (query.LecturerId.HasValue)
			eventsQuery = eventsQuery.Where(e => e.EventLecturers.Any(el => el.LecturerId == query.LecturerId.Value));

		if (query.FromUtc.HasValue)
			eventsQuery = eventsQuery.Where(e => e.EndUtc >= query.FromUtc.Value);

		if (query.ToUtc.HasValue)
			eventsQuery = eventsQuery.Where(e => e.StartUtc <= query.ToUtc.Value);

		if (!query.IncludeCancelled)
			eventsQuery = eventsQuery.Where(e => e.EventStatus.Name != "Cancelled");

		var results = await eventsQuery
			.OrderBy(e => e.StartUtc)
			.Select(e => new TimetableEventListItemDto
			{
				TimetableEventId = e.TimetableEventId,
				RecurrenceGroupId = e.RecurrenceGroupId,
				ModuleCode = e.Module.Code,
				ModuleTitle = e.Module.Title,
				RoomCode = e.Room.Code,
				RoomName = e.Room.Name,
				StartUtc = e.StartUtc,
				EndUtc = e.EndUtc,
				SessionType = e.SessionType,
				EventStatus = e.EventStatus.Name,
				CohortCodes = e.EventCohorts.Select(ec => ec.Cohort.Code).ToList(),
				LecturerNames = e.EventLecturers.Select(el => el.Lecturer.Name + " " + el.Lecturer.LastName).ToList()
			})
			.ToListAsync();

		return Ok(results);
	}
	[HttpGet("events/{id:long}/recurrence-group")]
	public async Task<IActionResult> GetRecurrenceGroupForEvent(long id)
	{
		var groupId = await _db.TimetableEvents
			.AsNoTracking()
			.Where(te => te.TimetableEventId == id)
			.Select(te => te.RecurrenceGroupId)
			.FirstOrDefaultAsync();

		if (groupId is null)
			return NotFound($"Event {id} not found or is not part of a recurring series.");

		return Ok(new { recurrenceGroupId = groupId });
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
			.Include(te => te.Module)
			.Include(te => te.Room)
				.ThenInclude(r => r.Building)
			.Include(te => te.EventCohorts)
				.ThenInclude(ec => ec.Cohort)
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
            var cohortNames = timetableEvent.EventCohorts
                .Select(ec => ec.Cohort.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .ToList();

            var cohortsText = cohortNames.Count > 0
                ? string.Join(", ", cohortNames)
                : "Not specified";

            var roomText = string.IsNullOrWhiteSpace(timetableEvent.Room.Name)
                ? timetableEvent.Room.Code
                : $"{timetableEvent.Room.Code} - {timetableEvent.Room.Name}";

            var buildingText = timetableEvent.Room.Building?.Name ?? "Not specified";

            var cancellationReason = string.IsNullOrWhiteSpace(dto.Reason)
                ? "Cancelled by admin."
                : dto.Reason;

            var cancellationMessage =
                $"Module: {timetableEvent.Module.Code} - {timetableEvent.Module.Title}\n" +
                $"Date: {timetableEvent.StartUtc:dddd, dd MMMM yyyy}\n" +
                $"Time: {timetableEvent.StartUtc:HH:mm} - {timetableEvent.EndUtc:HH:mm}\n" +
                $"Room: {roomText}\n" +
                $"Building: {buildingText}\n" +
                $"Cohorts: {cohortsText}\n" +
                $"Reason: {cancellationReason}";

            await _notificationService.CreateAsync(
                notificationTypeName: "EventCancelled",
                title: $"Class cancelled - {timetableEvent.Module.Code}",
                message: cancellationMessage,
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