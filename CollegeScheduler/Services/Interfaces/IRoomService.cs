using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Facilities;

namespace CollegeScheduler.Services.Interfaces
{
    public interface IRoomService
    {
        Task<PagedResult<RoomDto>?> GetByBuildingAsync(int buildingId, int page = 1, int pageSize = 10, string? search = null);
        Task<RoomDto?> GetByIdAsync(int roomId);
        Task<bool> CreateAsync(int buildingId, RoomCreateDto dto);
        Task<bool> UpdateAsync(int roomId, RoomUpdateDto dto);
        Task<bool> DeleteAsync(int roomId);
    }
}