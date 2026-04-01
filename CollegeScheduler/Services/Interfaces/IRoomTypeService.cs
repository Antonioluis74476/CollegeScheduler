using CollegeScheduler.DTOs;
using CollegeScheduler.DTOs.Academic;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Facilities;

namespace CollegeScheduler.Services.Interfaces
{
    public interface IRoomTypeService
    {
        Task<PagedResult<RoomTypeDto>?> GetAllAsync(int page = 1, int pageSize = 10, string? search = null);

        Task<RoomTypeDto?> GetByIdAsync(int roomTypeId);

        Task<bool> CreateAsync(RoomTypeCreateDto dto);

        Task<bool> UpdateAsync(int roomTypeId, RoomTypeUpdateDto dto);

        Task<bool> DeleteAsync(int roomTypeId);
    }
}