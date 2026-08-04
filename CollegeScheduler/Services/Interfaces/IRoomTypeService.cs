namespace CollegeScheduler.Services.Interfaces
{
    public interface IRoomTypeService
    {
        Task<CollegeScheduler.DTOs.Common.PagedResult<CollegeScheduler.DTOs.Facilities.RoomTypeDto>?> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            string? search = null);

        Task<CollegeScheduler.DTOs.Facilities.RoomTypeDto?> GetByIdAsync(int roomTypeId);

        Task<bool> CreateAsync(CollegeScheduler.DTOs.Facilities.RoomTypeCreateDto dto);

        Task<bool> UpdateAsync(int roomTypeId, CollegeScheduler.DTOs.Facilities.RoomTypeUpdateDto dto);

        Task<bool> DeleteAsync(int roomTypeId);
    }
}