using CollegeScheduler.DTOs.Academic;

namespace CollegeScheduler.Services.Interfaces
{
    public interface ICohortService
    {
        Task<CollegeScheduler.DTOs.Common.PagedResult<CohortDto>?> GetByProgramAsync(
            int programId,
            int page = 1,
            int pageSize = 10,
            string? search = null);

        Task<CohortDto?> GetByIdAsync(int cohortId);
        Task<bool> CreateAsync(int programId, CohortCreateDto dto);
        Task<bool> UpdateAsync(int cohortId, CohortUpdateDto dto);
    }
}