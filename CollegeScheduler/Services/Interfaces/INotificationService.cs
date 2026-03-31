namespace CollegeScheduler.Services.Interfaces;

public interface INotificationService
{
	Task<long> CreateAsync(
		string notificationTypeName,
		string title,
		string message,
		IEnumerable<string> recipientUserIds,
		long? relatedTimetableEventId = null,
		long? relatedRequestId = null);
}