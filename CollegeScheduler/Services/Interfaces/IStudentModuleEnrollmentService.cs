using CollegeScheduler.DTOs.Membership;

namespace CollegeScheduler.Services.Interfaces;

public interface IStudentModuleEnrollmentService
{
    Task<IReadOnlyList<StudentModuleEnrollmentDto>> GetAllAsync();

    Task<StudentModuleEnrollmentDto?> GetAsync(
        int studentId,
        int moduleId,
        int termId);

    Task<bool> CreateAsync(
        StudentModuleEnrollmentCreateDto dto);

    Task<bool> UpdateAsync(
        int studentId,
        int moduleId,
        int termId,
        StudentModuleEnrollmentUpdateDto dto);

    Task<bool> DeleteAsync(
        int studentId,
        int moduleId,
        int termId);

    Task<IReadOnlyList<StudentModuleEnrollmentDto>> GetByStudentAsync(
        int studentId);
}