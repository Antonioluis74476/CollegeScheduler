using CollegeScheduler.DTOs.Membership;

namespace CollegeScheduler.Services.Interfaces;

public interface IStudentCohortMembershipService
{
    Task<IReadOnlyList<StudentCohortMembershipDto>> GetAllAsync();

    Task<StudentCohortMembershipDto?> GetAsync(
        int studentId,
        int cohortId,
        int academicYearId);

    Task<bool> CreateAsync(StudentCohortMembershipCreateDto dto);

    Task<bool> UpdateAsync(
        int studentId,
        int cohortId,
        int academicYearId,
        StudentCohortMembershipUpdateDto dto);

    Task<bool> DeleteAsync(
        int studentId,
        int cohortId,
        int academicYearId);

    Task<IReadOnlyList<StudentCohortMembershipDto>> GetByStudentAsync(
        int studentId);
}