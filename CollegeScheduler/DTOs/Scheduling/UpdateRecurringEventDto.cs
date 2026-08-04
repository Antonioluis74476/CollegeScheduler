using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Scheduling;

public sealed class UpdateRecurringEventDto : IValidatableObject
{
	public RecurringEventUpdateScope Scope { get; set; }

	/// <summary>Required for ThisOnly / ThisAndFollowing — identifies which occurrence anchors the scope.</summary>
	public long? AnchorEventId { get; set; }

	public int? RoomId { get; set; }

	/// <summary>Time-of-day only. Applied to each matched occurrence's existing date.</summary>
	public TimeSpan? NewStartTime { get; set; }
	public TimeSpan? NewEndTime { get; set; }

	/// <summary>If provided, fully replaces the cohort set on each matched event.</summary>
	public List<int>? CohortIds { get; set; }

	/// <summary>If provided, fully replaces the lecturer set on each matched event.</summary>
	public List<int>? LecturerIds { get; set; }

	public string? SessionType { get; set; }
	public string? Notes { get; set; }
	public string? Reason { get; set; }

	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (Scope != RecurringEventUpdateScope.All && AnchorEventId is null)
		{
			yield return new ValidationResult(
				"AnchorEventId is required when Scope is ThisOnly or ThisAndFollowing.",
				new[] { nameof(AnchorEventId) });
		}

		if (NewStartTime.HasValue != NewEndTime.HasValue)
		{
			yield return new ValidationResult(
				"NewStartTime and NewEndTime must be provided together.",
				new[] { nameof(NewStartTime), nameof(NewEndTime) });
		}

		if (NewStartTime.HasValue && NewEndTime.HasValue && NewEndTime.Value <= NewStartTime.Value)
		{
			yield return new ValidationResult(
				"NewEndTime must be greater than NewStartTime.",
				new[] { nameof(NewEndTime) });
		}
	}
}