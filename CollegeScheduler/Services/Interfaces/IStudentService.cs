using CollegeScheduler.DTOs.Profiles;
using CollegeScheduler.DTOs.Requests;
using CollegeScheduler.DTOs.Student;
using CollegeScheduler.DTOs.Scheduling;

using CollegeScheduler.DTOs.Facilities;

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

    Task<List<AvailableRoomDto>> GetAvailableRoomsAsync(
    RoomSearchQuery query);

    Task<List<RecurringAvailableRoomDto>> GetRecurringAvailableRoomsAsync(
    RecurringRoomSearchQuery query);

    Task<List<CampusDto>> GetCampusesAsync();

    Task<List<CollegeScheduler.DTOs.Facilities.BuildingDto>>
        GetBuildingsByCampusAsync(int campusId);

    Task<IReadOnlyList<StudentRequestDto>> GetRequestsAsync();

	Task<ApiMessageResponseDto?> ChangePasswordAsync(
		ChangePasswordDto dto);



}