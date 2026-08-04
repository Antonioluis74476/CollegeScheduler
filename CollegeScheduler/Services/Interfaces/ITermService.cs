using CollegeScheduler.DTOs.Academic;
using CollegeScheduler.DTOs.Common;

namespace CollegeScheduler.Services.Interfaces
{
    public interface ITermService
    {
        Task<PagedResult<TermDto>?> GetByAcademicYearAsync(int academicYearId, int page = 1, int pageSize = 10, string? search = null);
        Task<TermDto?> GetByIdAsync(int termId);
        Task<bool> CreateAsync(int academicYearId, TermCreateDto dto);
        Task<bool> UpdateAsync(int termId, TermUpdateDto dto);
    }
}