using CollegeScheduler.DTOs.Audit;
using CollegeScheduler.DTOs.Common;

namespace CollegeScheduler.Services;

public interface IAuditLogService
{
    Task<CollegeScheduler.DTOs.Common.PagedResult<AuditLogDto>?> GetAuditLogsAsync(
        int page = 1,
        int pageSize = 20,
        string? userId = null,
        string? action = null,
        string? entityType = null,
        string? entityId = null,
        string sortDir = "desc");


    Task<AuditLogDto?> GetAuditLogByIdAsync(long id);

}