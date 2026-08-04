namespace CollegeScheduler.DTOs.Scheduling;

public class RecurringAvailableRoomDto
{
    public int RoomId { get; set; }

    public string RoomCode { get; set; } = string.Empty;

    public string RoomName { get; set; } = string.Empty;

    public string BuildingName { get; set; } = string.Empty;

    public string CampusName { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public string RoomType { get; set; } = string.Empty;

    public List<string> Features { get; set; } = [];

    public int AvailableOccurrences { get; set; }

    public int TotalOccurrences { get; set; }

    public List<UnavailableOccurrenceDto> UnavailableOccurrences { get; set; } = [];
}