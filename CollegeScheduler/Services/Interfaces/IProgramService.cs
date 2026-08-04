using CollegeScheduler.DTOs.Academic;

namespace CollegeScheduler.Services.Interfaces
{
    public interface IProgramService
    {
        Task<CollegeScheduler.DTOs.Common.PagedResult<ProgramDto>?> GetByDepartmentAsync(
            int departmentId,
            int page = 1,
            int pageSize = 10,
            string? search = null);

        Task<ProgramDto?> GetByIdAsync(int programId);
        Task<bool> CreateAsync(int departmentId, ProgramCreateDto dto);
        Task<bool> UpdateAsync(int programId, ProgramUpdateDto dto);
    }
}

