using CollegeScheduler.Data.Entities.Academic;
using CollegeScheduler.Data.Entities.Facilities;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Scheduling;

public sealed class TimetableEvent
{
	[Key]
	public long TimetableEventId { get; set; }

	public int TermId { get; set; }
	public int ModuleId { get; set; }
	public int RoomId { get; set; }

	public DateTime StartUtc { get; set; }
	public DateTime EndUtc { get; set; }

	public int EventStatusId { get; set; }

	[StringLength(20)]
	public string SessionType { get; set; } = "Lecture"; // Lecture/Lab/Tutorial/Exam

	public Guid? RecurrenceGroupId { get; set; }

	public string? Notes { get; set; }

	[Required]
	public string CreatedByUserId { get; set; } = "";

	public DateTime CreatedAtUtc { get; set; }
	public DateTime UpdatedAtUtc { get; set; }

	// Navigations
	public Term Term { get; set; } = null!;
	public Module Module { get; set; } = null!;
	public Room Room { get; set; } = null!;
	public EventStatus EventStatus { get; set; } = null!;

	public ApplicationUser? CreatedByUser { get; set; }

	public ICollection<EventCohort> EventCohorts { get; set; } = new List<EventCohort>();
	public ICollection<EventLecturer> EventLecturers { get; set; } = new List<EventLecturer>();
	public ICollection<TimetableEventChange> Changes { get; set; } = new List<TimetableEventChange>();
}