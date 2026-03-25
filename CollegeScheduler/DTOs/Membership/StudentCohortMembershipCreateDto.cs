namespace CollegeScheduler.DTOs.Membership
{
	public sealed class StudentCohortMembershipCreateDto
	{
		public int StudentId { get; set; }
		public int CohortId { get; set; }
		public int AcademicYearId { get; set; }

		public string MembershipType { get; set; } = "";
		public DateOnly? StartDate { get; set; }
		public DateOnly? EndDate { get; set; }
	}
}