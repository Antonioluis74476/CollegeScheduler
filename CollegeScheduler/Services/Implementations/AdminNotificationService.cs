using System.Net.Http.Json;
using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Notifications;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations;

public class AdminNotificationService : IAdminNotificationService
{
    private readonly HttpClient _http;

    public AdminNotificationService(HttpClient http)
    {
        _http = http;
    }

    public async Task<PagedResult<NotificationDto>?> GetNotificationsAsync(
        string? search = null,
        int? notificationTypeId = null,
        bool? isActive = true,
        int page = 1,
        int pageSize = 100)
    {
        var url =
            $"api/v1/admin/notifications?page={page}&pageSize={pageSize}&isActive={isActive}";

        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";

        if (notificationTypeId.HasValue)
            url += $"&notificationTypeId={notificationTypeId.Value}";

        return await _http.GetFromJsonAsync<PagedResult<NotificationDto>>(url);
    }

    // We'll implement these next
    public async Task<NotificationDto?> GetNotificationByIdAsync(long notificationId)
    {
        return await _http.GetFromJsonAsync<NotificationDto>(
            $"api/v1/admin/notifications/{notificationId}"
        );
    }

    public async Task<List<NotificationTypeDto>> GetNotificationTypesAsync()
    {
        var result = await _http.GetFromJsonAsync<PagedResult<NotificationTypeDto>>(
            "api/v1/admin/notification-types?page=1&pageSize=100"
        );

        return result?.Items ?? new List<NotificationTypeDto>();
    }

    public async Task<NotificationDto?> CreateNotificationAsync(NotificationCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync(
            "api/v1/admin/notifications",
            dto
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception(error);
        }

        return await response.Content.ReadFromJsonAsync<NotificationDto>();
    }
}