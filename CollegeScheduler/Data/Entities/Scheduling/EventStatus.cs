using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.Data.Entities.Scheduling;

public sealed class EventStatus
{
	[Key]
	public int EventStatusId { get; set; }

	[Required, StringLength(20)]
	public string Name { get; set; } = "";
}