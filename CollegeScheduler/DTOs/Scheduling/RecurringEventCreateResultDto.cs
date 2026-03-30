namespace CollegeScheduler.DTOs.Scheduling
{
    public class RecurringEventCreateResultDto
    {
        public int CreatedCount { get; set; }
        public Guid? RecurrenceGroupId { get; set; }
        public List<int> EventIds { get; set; } = new();
    }
}