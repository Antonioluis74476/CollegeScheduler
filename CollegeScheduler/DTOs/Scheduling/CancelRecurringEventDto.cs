using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Scheduling;

public sealed class CancelRecurringEventDto : IValidatableObject
{
	public RecurringEventUpdateScope Scope { get; set; }
	public long? AnchorEventId { get; set; }
	public string? Reason { get; set; }

	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (Scope != RecurringEventUpdateScope.All && AnchorEventId is null)
		{
			yield return new ValidationResult(
				"AnchorEventId is required when Scope is ThisOnly or ThisAndFollowing.",
				new[] { nameof(AnchorEventId) });
		}
	}
}