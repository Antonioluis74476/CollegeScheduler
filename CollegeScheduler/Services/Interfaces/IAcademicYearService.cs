using CollegeScheduler.DTOs.Academic;
using CollegeScheduler.DTOs.Common;

namespace CollegeScheduler.Services.Interfaces
{
    public interface IAcademicYearService
    {
        Task<PagedResult<AcademicYearDto>?> GetAllAsync(int page = 1, int pageSize = 10, string? search = null);
        Task<AcademicYearDto?> GetByIdAsync(int academicYearId);
        Task<bool> CreateAsync(AcademicYearCreateDto dto);
        Task<bool> UpdateAsync(int academicYearId, AcademicYearUpdateDto dto);
        Task<bool> SetCurrentAsync(int academicYearId);
    }
}