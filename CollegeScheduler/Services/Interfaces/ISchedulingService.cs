using CollegeScheduler.Data.Entities.Scheduling;
using CollegeScheduler.DTOs.Scheduling;


namespace CollegeScheduler.Services.Interfaces;

public interface ISchedulingService
{
	Task<ClashResult> CheckClashesAsync(
		long? excludeEventId,
		int roomId,
		DateTime startUtc,
		DateTime endUtc,
		IEnumerable<int> cohortIds,
		IEnumerable<int> lecturerIds);

	Task<List<AvailableRoomDto>> FindAvailableRoomsAsync(RoomSearchQuery query);

	Task<List<TimetableEvent>> GenerateRecurringEventsAsync(
		RecurringEventCreateDto dto,
		string createdByUserId);

	// NEW Recurring Events CRUD Operations
	Task<List<TimetableEvent>> GetRecurringEventSeriesAsync(Guid recurrenceGroupId);

	Task<RecurringEventUpdateResultDto> UpdateRecurringEventsAsync(
		Guid recurrenceGroupId,
		UpdateRecurringEventDto dto,
		string updatedByUserId);

	Task<RecurringEventUpdateResultDto> CancelRecurringEventsAsync(
		Guid recurrenceGroupId,
		CancelRecurringEventDto dto,
		string cancelledByUserId);
	
}