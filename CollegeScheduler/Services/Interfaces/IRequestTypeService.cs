using CollegeScheduler.DTOs.Requests;

namespace CollegeScheduler.Services.Interfaces;

public interface IRequestTypeService
{
    Task<List<RequestTypeDto>> GetAllAsync();
    Task<RequestTypeDto?> GetByIdAsync(int id);
    Task<bool> CreateAsync(RequestTypeCreateDto dto);
    Task<bool> UpdateAsync(int id, RequestTypeUpdateDto dto);
}