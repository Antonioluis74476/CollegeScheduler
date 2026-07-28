using CollegeScheduler.DTOs.Academic;
using CollegeScheduler.DTOs.Common;

namespace CollegeScheduler.Services.Interfaces;

public interface IModuleLecturerService
{
    Task<PagedResult<ModuleLecturerDto>?> GetAllAsync(
        int page = 1,
        int pageSize = 100);

    Task<ModuleLecturerDto?> GetByIdAsync(
        int moduleId,
        int lecturerId,
        int termId);

    Task<bool> CreateAsync(ModuleLecturerCreateDto dto);

    Task<bool> UpdateAsync(
        int moduleId,
        int lecturerId,
        int termId,
        ModuleLecturerUpdateDto dto);

    Task<bool> DeleteAsync(
        int moduleId,
        int lecturerId,
        int termId);
}