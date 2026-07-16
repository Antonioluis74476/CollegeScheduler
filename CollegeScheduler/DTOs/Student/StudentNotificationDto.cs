namespace CollegeScheduler.DTOs.Student;

public sealed class StudentNotificationDto
{
	public long NotificationId { get; set; }

	public string Title { get; set; } = "";

	public string Message { get; set; } = "";

	public DateTime CreatedAtUtc { get; set; }

	public DateTime? ReadAtUtc { get; set; }

	public long? RelatedTimetableEventId { get; set; }

	public long? RelatedRequestId { get; set; }

	public bool IsRead => ReadAtUtc.HasValue;
}