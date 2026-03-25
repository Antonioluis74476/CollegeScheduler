using System.ComponentModel.DataAnnotations;

namespace CollegeScheduler.DTOs.Requests;

public sealed class RequestUpdateDto
{
	[Required]
	public int RequestTypeId { get; set; }

	[Required]
	public int RequestStatusId { get; set; }

	[StringLength(200)]
	public string? Title { get; set; }

	public string? Notes { get; set; }

	public bool IsActive { get; set; } = true;
}