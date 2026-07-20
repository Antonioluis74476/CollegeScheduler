using CollegeScheduler.DTOs.Common;
using CollegeScheduler.DTOs.Notifications;

namespace CollegeScheduler.Services.Interfaces;

public interface IAdminNotificationService
{
    Task<PagedResult<NotificationDto>?> GetNotificationsAsync(
        string? search = null,
        int? notificationTypeId = null,
        bool? isActive = true,
        int page = 1,
        int pageSize = 100);

    Task<NotificationDto?> GetNotificationByIdAsync(long notificationId);

    Task<List<NotificationTypeDto>> GetNotificationTypesAsync();

    Task<NotificationDto?> CreateNotificationAsync(NotificationCreateDto dto);
}