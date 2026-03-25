using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Requests;

public sealed class RequestScheduleChangeUpdateDto
{
	[Required]
	public long TimetableEventId { get; set; }

	public int? ProposedRoomId { get; set; }
	public DateTime? ProposedStartUtc { get; set; }
	public DateTime? ProposedEndUtc { get; set; }

	[Required]
	[StringLength(800)]
	public string Reason { get; set; } = string.Empty;

	public bool IsActive { get; set; } = true;
}