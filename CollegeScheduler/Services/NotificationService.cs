using CollegeScheduler.Data;
using CollegeScheduler.Data.Entities.Notifications;
using CollegeScheduler.Messaging;
using CollegeScheduler.Services.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Services;

public sealed class NotificationService : INotificationService
{
	private readonly ApplicationDbContext _db;
	private readonly IPublishEndpoint _publishEndpoint;
	private readonly ILogger<NotificationService> _logger;

	public NotificationService(
		ApplicationDbContext db,
		IPublishEndpoint publishEndpoint,
		ILogger<NotificationService> logger)
	{
		_db = db;
		_publishEndpoint = publishEndpoint;
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

		// Get email addresses for recipients
		var userEmails = await _db.Users
			.Where(u => recipients.Contains(u.Id) && u.Email != null)
			.Select(u => new { u.Id, u.Email })
			.ToListAsync();

		// Publish an email job to RabbitMQ for each user email
		foreach (var user in userEmails)
		{
			await _publishEndpoint.Publish(new SendEmailMessage(
				To: user.Email!,
				Subject: title,
				Body: message));
		}

		_logger.LogInformation(
			"Notification created. NotificationId={NotificationId}, Type={Type}, Recipients={RecipientCount}, EmailsQueued={EmailCount}",
			notification.NotificationId,
			notificationTypeName,
			recipients.Count,
			userEmails.Count);

		return notification.NotificationId;
	}
}