using CollegeScheduler.DTOs.Audit;
using CollegeScheduler.DTOs.Common;
using System.Net.Http.Json;

namespace CollegeScheduler.Services;

public sealed class AuditLogService : IAuditLogService
{
    private readonly HttpClient _http;

    public AuditLogService(HttpClient http)
    {
        _http = http;
    }

    public async Task<CollegeScheduler.DTOs.Common.PagedResult<AuditLogDto>?> GetAuditLogsAsync(
        int page = 1,
        int pageSize = 20,
        string? userId = null,
        string? action = null,
        string? entityType = null,
        string? entityId = null,
        string sortDir = "desc")
    {
        var query = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}",
            $"sortDir={Uri.EscapeDataString(sortDir)}"
        };

        if (!string.IsNullOrWhiteSpace(userId))
            query.Add($"userId={Uri.EscapeDataString(userId)}");

        if (!string.IsNullOrWhiteSpace(action))
            query.Add($"action={Uri.EscapeDataString(action)}");

        if (!string.IsNullOrWhiteSpace(entityType))
            query.Add($"entityType={Uri.EscapeDataString(entityType)}");

        if (!string.IsNullOrWhiteSpace(entityId))
            query.Add($"entityId={Uri.EscapeDataString(entityId)}");

        var url = $"api/v1/admin/audit-logs?{string.Join("&", query)}";

        return await _http.GetFromJsonAsync<
            CollegeScheduler.DTOs.Common.PagedResult<AuditLogDto>>(url);
    }

    public async Task<AuditLogDto?> GetAuditLogByIdAsync(long id)
    {
        return await _http.GetFromJsonAsync<AuditLogDto>(
            $"api/v1/admin/audit-logs/{id}");
    }


}