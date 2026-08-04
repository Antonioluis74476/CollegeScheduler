namespace CollegeScheduler.Services.Interfaces
{
    public interface IFeatureService
    {
        Task<CollegeScheduler.DTOs.Common.PagedResult<CollegeScheduler.DTOs.Facilities.FeatureDto>?> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            string? search = null);

        Task<CollegeScheduler.DTOs.Facilities.FeatureDto?> GetByIdAsync(int featureId);

        Task<bool> CreateAsync(CollegeScheduler.DTOs.Facilities.FeatureCreateDto dto);

        Task<bool> UpdateAsync(int featureId, CollegeScheduler.DTOs.Facilities.FeatureUpdateDto dto);

        Task<bool> DeleteAsync(int featureId);
    }
}