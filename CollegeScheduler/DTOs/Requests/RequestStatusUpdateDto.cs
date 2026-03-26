using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Requests;

public sealed class RequestStatusUpdateDto
{
	[Required]
	[StringLength(20)]
	public string Name { get; set; } = string.Empty;

	public bool IsActive { get; set; } = true;
}