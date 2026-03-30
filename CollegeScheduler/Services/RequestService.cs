using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Requests;
using CollegeScheduler.Data.Entities.Scheduling;
using CollegeScheduler.DTOs.Requests;
using CollegeScheduler.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using CollegeScheduler.Hubs;

namespace CollegeScheduler.Services;

public sealed class RequestService : IRequestService
{
	private readonly ApplicationDbContext _db;
	private readonly ISchedulingService _schedulingService;
	private readonly INotificationService _notificationService;
	private readonly TimetableHubNotifier _hubNotifier;
	private readonly ILogger<RequestService> _logger;

	public RequestService(
		ApplicationDbContext db,
		ISchedulingService schedulingService,
		INotificationService notificationService,
		TimetableHubNotifier hubNotifier,
		ILogger<RequestService> logger)
	{
		_db = db;
		_schedulingService = schedulingService;
		_notificationService = notificationService;
		_hubNotifier = hubNotifier;
		_logger = logger;
	}

	public async Task<long> CreateScheduleChangeRequestAsync(
		string requestedByUserId,
		long timetableEventId,
		int? proposedRoomId,
		DateTime? proposedStartUtc,
		DateTime? proposedEndUtc,
		string reason)
	{
		if (string.IsNullOrWhiteSpace(requestedByUserId))
			throw new ArgumentException("RequestedByUserId is required.");

		if (string.IsNullOrWhiteSpace(reason))
			throw new ArgumentException("Reason is required.");

		var eventExists = await _db.TimetableEvents
			.AnyAsync(x => x.TimetableEventId == timetableEventId);

		if (!eventExists)
			throw new ArgumentException($"TimetableEvent {timetableEventId} not found.");

		if (proposedStartUtc.HasValue && proposedEndUtc.HasValue && proposedEndUtc <= proposedStartUtc)
			throw new ArgumentException("ProposedEndUtc must be greater than ProposedStartUtc.");

		var requestTypeId = await _db.RequestTypes
			.Where(x => x.Name == "Reschedule")
			.Select(x => x.RequestTypeId)
			.FirstOrDefaultAsync();

		if (requestTypeId == 0)
			throw new InvalidOperationException("RequestType 'Reschedule' not found.");

		var pendingStatusId = await _db.RequestStatuses
			.Where(x => x.Name == "Pending")
			.Select(x => x.RequestStatusId)
			.FirstOrDefaultAsync();

		if (pendingStatusId == 0)
			throw new InvalidOperationException("RequestStatus 'Pending' not found.");

		var request = new Request
		{
			RequestTypeId = requestTypeId,
			RequestStatusId = pendingStatusId,
			RequestedByUserId = requestedByUserId,
			Title = $"Schedule change request for event {timetableEventId}",
			Notes = reason
		};

		_db.Requests.Add(request);
		await _db.SaveChangesAsync();

		var detail = new RequestScheduleChange
		{
			RequestId = request.RequestId,
			TimetableEventId = timetableEventId,
			ProposedRoomId = proposedRoomId,
			ProposedStartUtc = proposedStartUtc,
			ProposedEndUtc = proposedEndUtc,
			Reason = reason
		};

		_db.RequestScheduleChanges.Add(detail);
		await _db.SaveChangesAsync();

		_logger.LogInformation(
			"Schedule change request created. RequestId={RequestId}, EventId={EventId}, RequestedBy={RequestedBy}",
			request.RequestId,
			timetableEventId,
			requestedByUserId);

		return request.RequestId;
	}

	public async Task<long> CreateRoomBookingRequestAsync(
		string requestedByUserId,
		int roomId,
		DateTime startUtc,
		DateTime endUtc,
		string purpose,
		int expectedAttendees)
	{
		if (string.IsNullOrWhiteSpace(requestedByUserId))
			throw new ArgumentException("RequestedByUserId is required.");

		if (string.IsNullOrWhiteSpace(purpose))
			throw new ArgumentException("Purpose is required.");

		if (endUtc <= startUtc)
			throw new ArgumentException("EndUtc must be greater than StartUtc.");

		if (expectedAttendees < 1)
			throw new ArgumentException("ExpectedAttendees must be at least 1.");

		var room = await _db.Rooms
			.Where(r => r.RoomId == roomId && r.IsActive)
			.Select(r => new
			{
				r.RoomId,
				r.Capacity,
				r.IsBookableByStudents
			})
			.FirstOrDefaultAsync();

		if (room is null)
			throw new ArgumentException($"Room {roomId} not found.");

		if (!room.IsBookableByStudents)
			throw new InvalidOperationException("This room is not bookable by students.");

		if (expectedAttendees > room.Capacity)
			throw new InvalidOperationException("Expected attendees exceed room capacity.");

		var requestTypeId = await _db.RequestTypes
			.Where(x => x.Name == "RoomBooking")
			.Select(x => x.RequestTypeId)
			.FirstOrDefaultAsync();

		if (requestTypeId == 0)
			throw new InvalidOperationException("RequestType 'RoomBooking' not found.");

		var pendingStatusId = await _db.RequestStatuses
			.Where(x => x.Name == "Pending")
			.Select(x => x.RequestStatusId)
			.FirstOrDefaultAsync();

		if (pendingStatusId == 0)
			throw new InvalidOperationException("RequestStatus 'Pending' not found.");

		var request = new Request
		{
			RequestTypeId = requestTypeId,
			RequestStatusId = pendingStatusId,
			RequestedByUserId = requestedByUserId,
			Title = $"Room booking request for room {roomId}",
			Notes = purpose
		};

		_db.Requests.Add(request);
		await _db.SaveChangesAsync();

		var detail = new RequestRoomBooking
		{
			RequestId = request.RequestId,
			RoomId = roomId,
			StartUtc = startUtc,
			EndUtc = endUtc,
			Purpose = purpose,
			ExpectedAttendees = expectedAttendees
		};

		_db.RequestRoomBookings.Add(detail);
		await _db.SaveChangesAsync();

		_logger.LogInformation(
			"Room booking request created. RequestId={RequestId}, RoomId={RoomId}, RequestedBy={RequestedBy}",
			request.RequestId,
			roomId,
			requestedByUserId);

		return request.RequestId;
	}

	public async Task<DecisionResultDto> DecideAsync(
		long requestId,
		string decidedByUserId,
		string decision,
		string? comment)
	{
		if (decision is not ("Approved" or "Rejected"))
			throw new ArgumentException("Decision must be 'Approved' or 'Rejected'.");

		var request = await _db.Requests
			.Include(r => r.RequestType)
			.Include(r => r.RequestStatus)
			.FirstOrDefaultAsync(r => r.RequestId == requestId);

		if (request is null)
			throw new ArgumentException($"Request {requestId} not found.");

		if (request.RequestStatus.Name is "Approved" or "Rejected")
		{
			return new DecisionResultDto
			{
				IsSuccess = false,
				Message = $"Request has already been {request.RequestStatus.Name.ToLower()}."
			};
		}

		var targetStatusId = await _db.RequestStatuses
			.Where(x => x.Name == decision)
			.Select(x => x.RequestStatusId)
			.FirstOrDefaultAsync();

		if (targetStatusId == 0)
			throw new InvalidOperationException($"RequestStatus '{decision}' not found.");

		_db.RequestDecisions.Add(new RequestDecision
		{
			RequestId = requestId,
			DecidedByUserId = decidedByUserId,
			Decision = decision,
			Comment = comment,
			DecidedAtUtc = DateTime.UtcNow
		});

		request.RequestStatusId = targetStatusId;
		await _db.SaveChangesAsync();

		if (decision == "Approved")
		{
			if (request.RequestType.Name == "Reschedule")
			{
				await ApplyApprovedScheduleChangeAsync(requestId, decidedByUserId);
			}
			else if (request.RequestType.Name == "RoomBooking")
			{
				await ApplyApprovedRoomBookingAsync(requestId);
			}
		}

		await _notificationService.CreateAsync(
			notificationTypeName: decision == "Approved" ? "RequestApproved" : "RequestRejected",
			title: $"Request {decision}",
			message: string.IsNullOrWhiteSpace(comment)
				? $"Your request #{requestId} was {decision.ToLower()}."
				: $"Your request #{requestId} was {decision.ToLower()}. Comment: {comment}",
			recipientUserIds: new[] { request.RequestedByUserId },
			relatedRequestId: requestId);

		await _hubNotifier.PushRequestDecisionAsync(
			requesterUserId: request.RequestedByUserId,
			requestId: requestId,
			decision: decision,
			comment: comment);

		_logger.LogInformation(
			"Request decided. RequestId={RequestId}, Decision={Decision}, DecidedBy={DecidedBy}",
			requestId,
			decision,
			decidedByUserId);

		return new DecisionResultDto
		{
			IsSuccess = true,
			Message = $"Request {decision.ToLower()} successfully."
		};
	}

	private async Task ApplyApprovedScheduleChangeAsync(long requestId, string changedByUserId)
	{
		var detail = await _db.RequestScheduleChanges
			.Include(x => x.TimetableEvent)
				.ThenInclude(te => te.EventCohorts)
			.Include(x => x.TimetableEvent)
				.ThenInclude(te => te.EventLecturers)
			.FirstOrDefaultAsync(x => x.RequestId == requestId);

		if (detail is null)
			throw new InvalidOperationException($"RequestScheduleChange for request {requestId} not found.");

		var timetableEvent = detail.TimetableEvent;
		if (timetableEvent is null)
			throw new InvalidOperationException("TimetableEvent not found for approved schedule change.");

		var newRoomId = detail.ProposedRoomId ?? timetableEvent.RoomId;
		var newStartUtc = detail.ProposedStartUtc ?? timetableEvent.StartUtc;
		var newEndUtc = detail.ProposedEndUtc ?? timetableEvent.EndUtc;

		var cohortIds = timetableEvent.EventCohorts.Select(ec => ec.CohortId).ToList();
		var lecturerIds = timetableEvent.EventLecturers.Select(el => el.LecturerId).ToList();

		var clashResult = await _schedulingService.CheckClashesAsync(
			excludeEventId: timetableEvent.TimetableEventId,
			roomId: newRoomId,
			startUtc: newStartUtc,
			endUtc: newEndUtc,
			cohortIds: cohortIds,
			lecturerIds: lecturerIds);

		if (clashResult.HasClash)
		{
			throw new InvalidOperationException("Approved schedule change cannot be applied because it creates a clash.");
		}

		var oldRoomId = timetableEvent.RoomId;
		var oldStartUtc = timetableEvent.StartUtc;
		var oldEndUtc = timetableEvent.EndUtc;

		timetableEvent.RoomId = newRoomId;
		timetableEvent.StartUtc = newStartUtc;
		timetableEvent.EndUtc = newEndUtc;

		_db.TimetableEventChanges.Add(new TimetableEventChange
		{
			TimetableEventId = timetableEvent.TimetableEventId,
			ChangeType = "Reschedule",
			OldRoomId = oldRoomId,
			NewRoomId = newRoomId,
			OldStartUtc = oldStartUtc,
			NewStartUtc = newStartUtc,
			OldEndUtc = oldEndUtc,
			NewEndUtc = newEndUtc,
			Reason = detail.Reason,
			ChangedByUserId = changedByUserId,
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
				message: $"Timetable event #{timetableEvent.TimetableEventId} has been rescheduled.",
				recipientUserIds: recipientUserIds.Distinct(),
				relatedTimetableEventId: timetableEvent.TimetableEventId,
				relatedRequestId: requestId);
		}

		await _hubNotifier.PushEventChangedAsync(
			timetableEventId: timetableEvent.TimetableEventId,
			cohortIds: cohortIds,
			lecturerUserIds: lecturerUserIds,
			oldStartUtc: oldStartUtc,
			newStartUtc: newStartUtc);

		var lastChange = await _db.TimetableEventChanges
			.Where(x => x.TimetableEventId == timetableEvent.TimetableEventId)
			.OrderByDescending(x => x.ChangedAtUtc)
			.FirstOrDefaultAsync();

		if (lastChange is not null)
		{
			lastChange.NotificationSent = true;
			await _db.SaveChangesAsync();
		}
	}

	private async Task ApplyApprovedRoomBookingAsync(long requestId)
	{
		var detail = await _db.RequestRoomBookings
			.FirstOrDefaultAsync(x => x.RequestId == requestId);

		if (detail is null)
			throw new InvalidOperationException($"RequestRoomBooking for request {requestId} not found.");

		// For MVP:
		// approved room booking remains as approved request row
		// and availability checks can later exclude approved bookings if needed
		await Task.CompletedTask;
	}
}