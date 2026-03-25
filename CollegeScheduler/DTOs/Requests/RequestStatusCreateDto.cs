using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Requests;

public sealed class RequestStatusCreateDto
{
	[Required]
	[StringLength(20)]
	public string Name { get; set; } = string.Empty;
}