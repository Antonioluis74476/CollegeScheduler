namespace CollegeScheduler.DTOs.Membership
{
	public sealed class StudentCohortMembershipUpdateDto
	{
		public string MembershipType { get; set; } = "";
		public DateOnly? StartDate { get; set; }
		public DateOnly? EndDate { get; set; }
	}
}