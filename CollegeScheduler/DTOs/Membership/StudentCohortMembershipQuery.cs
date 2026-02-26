namespace CollegeScheduler.DTOs.Membership
{
	public sealed class StudentCohortMembershipQuery
	{
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 20;

		public int? StudentId { get; set; }
		public int? CohortId { get; set; }
		public int? AcademicYearId { get; set; }
		public string? MembershipType { get; set; }
	}
}