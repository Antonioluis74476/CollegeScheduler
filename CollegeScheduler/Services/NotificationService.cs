using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Notifications;
using CollegeScheduler.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Services;

public sealed class NotificationService : INotificationService
{
	private readonly ApplicationDbContext _db;
	private readonly ILogger<NotificationService> _logger;

	public NotificationService(
		ApplicationDbContext db,
		ILogger<NotificationService> logger)
	{
		_db = db;
		_logger = logger;
	}

	public async Task<long> CreateAsync(
		string notificationTypeName,
		string title,
		string message,
		IEnumerable<string> recipientUserIds,
		long? relatedTimetableEventId = null,
		long? relatedRequestId = null)
	{
		var typeId = await _db.NotificationTypes
			.Where(x => x.Name == notificationTypeName)
			.Select(x => x.NotificationTypeId)
			.FirstOrDefaultAsync();

		if (typeId == 0)
			throw new InvalidOperationException($"NotificationType '{notificationTypeName}' not found.");

		var recipients = recipientUserIds
			.Where(x => !string.IsNullOrWhiteSpace(x))
			.Distinct()
			.ToList();

		if (recipients.Count == 0)
			throw new InvalidOperationException("At least one recipient is required.");

		var notification = new Notification
		{
			NotificationTypeId = typeId,
			Title = title,
			Message = message,
			RelatedTimetableEventId = relatedTimetableEventId,
			RelatedRequestId = relatedRequestId
		};

		_db.Notifications.Add(notification);
		await _db.SaveChangesAsync();

		foreach (var userId in recipients)
		{
			_db.NotificationRecipients.Add(new NotificationRecipient
			{
				NotificationId = notification.NotificationId,
				UserId = userId,
				DeliveryStatus = "Pending"
			});
		}

		await _db.SaveChangesAsync();

		_logger.LogInformation(
			"Notification created. NotificationId={NotificationId}, Type={Type}, Recipients={RecipientCount}",
			notification.NotificationId,
			notificationTypeName,
			recipients.Count);

		return notification.NotificationId;
	}
}