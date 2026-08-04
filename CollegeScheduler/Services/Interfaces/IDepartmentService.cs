using CollegeScheduler.DTOs.Academic;

namespace CollegeScheduler.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<CollegeScheduler.DTOs.Common.PagedResult<DepartmentDto>?> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            string? search = null);

        Task<DepartmentDto?> GetByIdAsync(int departmentId);
        Task<bool> CreateAsync(DepartmentCreateDto dto);
        Task<bool> UpdateAsync(int departmentId, DepartmentUpdateDto dto);
    }
}