namespace CollegeScheduler.DTOs.Scheduling;

public class UnavailableOccurrenceDto
{
    public DateTime StartUtc { get; set; }

    public DateTime EndUtc { get; set; }

    public string Reason { get; set; } = string.Empty;
}