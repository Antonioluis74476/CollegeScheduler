using CollegeScheduler.DTOs.Profiles;

namespace CollegeScheduler.Services.Interfaces;

public interface IAdminUserService
{
    Task<List<StudentDto>> GetStudentsAsync();
    Task<StudentDto?> CreateStudentAsync(StudentCreateDto dto);

    Task<List<LecturerDto>> GetLecturersAsync();
    Task<LecturerDto?> CreateLecturerAsync(LecturerCreateDto dto);

    Task UpdateStudentAsync(int id, StudentUpdateDto dto);

    Task UpdateLecturerAsync(int id, LecturerUpdateDto dto);

    Task ActivateStudentAsync(int id);
    Task DeactivateStudentAsync(int id);

    Task ActivateLecturerAsync(int id);
    Task DeactivateLecturerAsync(int id);

}