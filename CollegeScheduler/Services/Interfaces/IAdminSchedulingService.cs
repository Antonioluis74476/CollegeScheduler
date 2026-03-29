using CollegeScheduler.DTOs.Scheduling;

namespace CollegeScheduler.Services.Interfaces
{
    public interface IAdminSchedulingService
    {
        Task<List<AvailableRoomDto>?> GetAvailableRoomsAsync(RoomSearchQuery query);
        Task<ClashResult?> CheckClashesAsync(ClashCheckRequest request);
    }


}