using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Requests;

public sealed class RequestDecisionCreateDto
{
	[Required]
	public string DecidedByUserId { get; set; } = string.Empty;

	[Required]
	[StringLength(20)]
	public string Decision { get; set; } = string.Empty;

	[StringLength(500)]
	public string? Comment { get; set; }
}