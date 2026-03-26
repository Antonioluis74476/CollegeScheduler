using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Requests;

public sealed class RequestTypeUpdateDto
{
	[Required]
	[StringLength(30)]
	public string Name { get; set; } = string.Empty;

	public bool IsActive { get; set; } = true;
}