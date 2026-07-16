namespace CollegeScheduler.DTOs.Lecturer;

public sealed class LecturerNotificationDto
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