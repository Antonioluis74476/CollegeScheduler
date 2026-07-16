using CollegeScheduler.DTOs.Profiles;
using CollegeScheduler.DTOs.Requests;
using CollegeScheduler.DTOs.Student;

namespace CollegeScheduler.Services.Interfaces;

public interface IStudentService
{
	Task<StudentDto?> GetProfileAsync();

	Task<IReadOnlyList<StudentTimetableItemDto>> GetTimetableAsync(
		DateTime? fromUtc = null,
		DateTime? toUtc = null);

	Task<IReadOnlyList<StudentNotificationDto>> GetNotificationsAsync(
		bool unreadOnly = false);

	Task MarkNotificationAsReadAsync(long notificationId);

	Task<RoomBookingResponseDto?> CreateRoomBookingRequestAsync(
		RoomBookingRequestCreateDto dto);

	Task<IReadOnlyList<StudentRequestDto>> GetRequestsAsync();

	Task<ApiMessageResponseDto?> ChangePasswordAsync(
		ChangePasswordDto dto);
}