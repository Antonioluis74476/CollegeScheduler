using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Requests;

public sealed class RequestTypeCreateDto
{
	[Required]
	[StringLength(30)]
	public string Name { get; set; } = string.Empty;
}