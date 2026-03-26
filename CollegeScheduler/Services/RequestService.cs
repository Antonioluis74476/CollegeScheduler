using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Requests;
using CollegeScheduler.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Services;

public sealed class RequestService : IRequestService
{
	private readonly ApplicationDbContext _db;
	private readonly ILogger<RequestService> _logger;

	public RequestService(
		ApplicationDbContext db,
		ILogger<RequestService> logger)
	{
		_db = db;
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
}