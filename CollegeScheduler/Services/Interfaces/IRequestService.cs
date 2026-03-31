using CollegeScheduler.DTOs.Requests;

namespace CollegeScheduler.Services.Interfaces;

public interface IRequestService
{
	Task<long> CreateScheduleChangeRequestAsync(
		string requestedByUserId,
		long timetableEventId,
		int? proposedRoomId,
		DateTime? proposedStartUtc,
		DateTime? proposedEndUtc,
		string reason);

	Task<long> CreateCancelClassRequestAsync(
		string requestedByUserId,
		long timetableEventId,
		string reason);

	Task<long> CreateRoomBookingRequestAsync(
		string requestedByUserId,
		int roomId,
		DateTime startUtc,
		DateTime endUtc,
		string purpose,
		int expectedAttendees);

	Task<DecisionResultDto> DecideAsync(
		long requestId,
		string decidedByUserId,
		string decision,
		string? comment);
}