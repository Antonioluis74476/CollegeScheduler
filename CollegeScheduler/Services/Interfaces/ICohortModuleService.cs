using CollegeScheduler.DTOs.Academic;

namespace CollegeScheduler.Services.Interfaces
{
    public interface ICohortModuleService
    {
        Task<CollegeScheduler.DTOs.Common.PagedResult<CohortModuleDto>?> GetByCohortAsync(
            int cohortId,
            int page = 1,
            int pageSize = 10,
            string? search = null);

        Task<CohortModuleDto?> GetByIdAsync(int cohortModuleId);

        Task<bool> CreateAsync(int cohortId, CohortModuleCreateDto dto);

        Task<bool> UpdateAsync(int cohortModuleId, CohortModuleUpdateDto dto);
    }
}