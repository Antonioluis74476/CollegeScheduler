namespace CollegeScheduler.Services.Interfaces
{
    public interface IRoomFeatureService
    {
        Task<List<CollegeScheduler.DTOs.Facilities.RoomFeatureDto>?> GetByRoomIdAsync(int roomId);

        Task<bool> AddAsync(int roomId, CollegeScheduler.DTOs.Facilities.RoomFeatureAddDto dto);

        Task<bool> DeleteAsync(int roomId, int featureId);
    }
}