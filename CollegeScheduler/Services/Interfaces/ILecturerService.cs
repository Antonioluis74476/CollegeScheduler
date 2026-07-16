using CollegeScheduler.DTOs.Lecturer;
using CollegeScheduler.DTOs.Profiles;
using CollegeScheduler.DTOs.Requests;
using CollegeScheduler.DTOs.Scheduling;

namespace CollegeScheduler.Services.Interfaces;

public interface ILecturerService
{
	Task<LecturerProfileDto?> GetProfileAsync();

	Task<IReadOnlyList<LecturerTimetableItemDto>> GetTimetableAsync(
		DateTime? fromUtc = null,
		DateTime? toUtc = null);

	Task<IReadOnlyList<LecturerNotificationDto>> GetNotificationsAsync(
	bool unreadOnly = false);

	Task MarkNotificationAsReadAsync(long notificationId);

	Task<IReadOnlyList<LecturerRequestDto>> GetRequestsAsync();

	Task<LecturerRequestResponseDto?> CreateScheduleChangeRequestAsync(
		ScheduleChangeRequestCreateDto dto);

	Task<LecturerRequestResponseDto?> CreateCancelClassRequestAsync(
		CancelClassRequestCreateDto dto);

	Task<LecturerRequestResponseDto?> CreateRoomBookingRequestAsync(
		LecturerRoomBookingRequestCreateDto dto);

    Task<List<AvailableRoomDto>> GetAvailableRoomsAsync(
    RoomSearchQuery query);

    Task<ApiMessageResponseDto?> ChangePasswordAsync(
		ChangePasswordDto dto);
}