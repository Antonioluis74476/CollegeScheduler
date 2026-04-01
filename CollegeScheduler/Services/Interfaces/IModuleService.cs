using CollegeScheduler.DTOs.Academic;

namespace CollegeScheduler.Services.Interfaces
{
    public interface IModuleService
    {
        Task<CollegeScheduler.DTOs.Common.PagedResult<ModuleDto>?> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            string? search = null);

        Task<ModuleDto?> GetByIdAsync(int moduleId);
        Task<bool> CreateAsync(ModuleCreateDto dto);
        Task<bool> UpdateAsync(int moduleId, ModuleUpdateDto dto);
    }
}