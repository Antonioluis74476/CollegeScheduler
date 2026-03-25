using CollegeScheduler.Data.Entities.Facilities;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Scheduling;

public sealed class TimetableEventChange
{
	[Key]
	public long TimetableEventChangeId { get; set; }

	public long TimetableEventId { get; set; }

	[Required, StringLength(20)]
	public string ChangeType { get; set; } = ""; // RoomChange/TimeChange/Cancellation/LecturerChange

	public int? OldRoomId { get; set; }
	public int? NewRoomId { get; set; }

	public DateTime? OldStartUtc { get; set; }
	public DateTime? NewStartUtc { get; set; }

	public DateTime? OldEndUtc { get; set; }
	public DateTime? NewEndUtc { get; set; }

	[Required, StringLength(500)]
	public string Reason { get; set; } = "";

	[Required]
	public string ChangedByUserId { get; set; } = "";

	public DateTime ChangedAtUtc { get; set; }

	public bool NotificationSent { get; set; }

	// Navigations
	public TimetableEvent TimetableEvent { get; set; } = null!;
	public Room? OldRoom { get; set; }
	public Room? NewRoom { get; set; }
	public ApplicationUser? ChangedByUser { get; set; }
}