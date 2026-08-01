namespace CollegeScheduler.DTOs.Scheduling;

public class RecurringRoomSearchQuery
{
    public DateOnly StartDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public int DurationMinutes { get; set; }

    public int NumberOfWeeks { get; set; }

    public int MinimumCapacity { get; set; }

    public int? CampusId { get; set; }

    public int? BuildingId { get; set; }

    public int? RoomTypeId { get; set; }

    public List<int> RequiredFeatureIds { get; set; } = [];

    public List<DayOfWeek> DaysOfWeek { get; set; } = [];

}