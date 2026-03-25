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
}